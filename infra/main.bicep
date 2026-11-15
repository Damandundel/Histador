// Histador production environment: App Service (Linux, .NET 8) + Azure SQL + Application Insights.
// Deploy into an existing resource group (see "Deploy to Azure" in README.md):
//   az deployment group create -g <resource-group> -f infra/main.bicep -p appName=<name> sqlAdminLogin=<login> ...
// Secrets are passed as parameters at deploy time and end up only in App Service configuration, never in git.

@description('Base name for every resource. Must be globally unique; the site will be https://<appName>.azurewebsites.net.')
@minLength(3)
@maxLength(40)
param appName string

@description('Azure region. Defaults to the resource group\'s region.')
param location string = resourceGroup().location

@description('App Service plan size. B1 supports Always On and WebSockets for SignalR; F1 is free but sleeps after 20 idle minutes and allows only 5 WebSocket connections.')
@allowed([
  'F1'
  'B1'
  'B2'
])
param appServiceSku string = 'B1'

@description('SQL Server administrator login.')
param sqlAdminLogin string

@description('SQL Server administrator password.')
@secure()
param sqlAdminPassword string

@description('Email of the admin account created on first start (AdminSeed:Email). Leave empty to skip seeding.')
param adminSeedEmail string = ''

@description('Password of the admin account created on first start (AdminSeed:Password).')
@secure()
param adminSeedPassword string = ''

var sqlServerName = '${appName}-sql'
var sqlDatabaseName = 'Histador'

resource logs 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${appName}-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    workspaceCapping: {
      dailyQuotaGb: 1 // hard cap so a log storm can't eat the student credits
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${appName}-insights'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logs.id
  }
}

resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    version: '12.0'
  }
}

// 0.0.0.0 - 0.0.0.0 is Azure's special rule for "allow Azure services", so the App Service can connect.
// The deploy workflow adds a temporary rule for its own runner IP while it runs migrations.
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2021-11-01' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-11-01' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648 // 2 GB, the Basic tier maximum
    requestedBackupStorageRedundancy: 'Local'
  }
}

// Point-in-time restore: Azure SQL keeps automatic backups. 7 days is the Basic tier maximum.
resource sqlBackupRetention 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies@2021-11-01' = {
  parent: sqlDatabase
  name: 'default'
  properties: {
    retentionDays: 7
  }
}

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${appName}-plan'
  location: location
  kind: 'linux'
  sku: {
    name: appServiceSku
    capacity: 1 // single instance: SignalR duels need no backplane in v1
  }
  properties: {
    reserved: true // required for Linux plans
  }
}

resource site 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      webSocketsEnabled: true
      alwaysOn: appServiceSku != 'F1' // Always On is not available on the free tier
      healthCheckPath: '/health'
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
          value: 'true' // trust App Service's TLS-terminating proxy so the app sees https
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'AdminSeed__Email'
          value: adminSeedEmail
        }
        {
          name: 'AdminSeed__Password'
          value: adminSeedPassword
        }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          type: 'SQLAzure'
          connectionString: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};User ID=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
      ]
    }
  }
}

output webAppName string = site.name
output webAppUrl string = 'https://${site.properties.defaultHostName}'
output sqlServerName string = sqlServer.name
output sqlDatabaseName string = sqlDatabase.name
