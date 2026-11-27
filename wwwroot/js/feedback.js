(() => {
    "use strict";

    const MIN_LENGTH = 10;
    const MAX_LENGTH = 1000;
    const API_URL = "/api/feedback";

    const form = document.getElementById("feedback-form");
    if (!form) {
        return;
    }

    const typeInputs = Array.from(form.querySelectorAll('input[name="type"]'));
    const typeError = document.getElementById("feedback-type-error");
    const contentInput = document.getElementById("feedback-content");
    const contentError = document.getElementById("feedback-content-error");
    const counter = document.getElementById("feedback-counter");
    const status = document.getElementById("feedback-status");
    const submitButton = document.getElementById("feedback-submit");

    if (!typeError || !contentInput || !contentError || !counter || !status || !submitButton) {
        return;
    }

    const submitLabel = submitButton.textContent || "Send";

    let isSubmitting = false;
    let typeTouched = false;
    let contentTouched = false;

    function selectedType() {
        const selected = typeInputs.find(input => input.checked);
        return selected ? Number(selected.value) : null;
    }

    function trimmedContent() {
        return contentInput.value.trim();
    }

    function isValid() {
        const type = selectedType();
        const length = trimmedContent().length;
        return (type === 1 || type === 2) && length >= MIN_LENGTH && length <= MAX_LENGTH;
    }

    function updateCounter() {
        counter.textContent = `${contentInput.value.length} / ${MAX_LENGTH}`;
    }

    function updateSubmitButton() {
        submitButton.disabled = isSubmitting || !isValid();
        submitButton.textContent = isSubmitting ? "Sending..." : submitLabel;
        submitButton.setAttribute("aria-busy", isSubmitting ? "true" : "false");
    }

    function setTypeError(message) {
        typeError.textContent = message || "";
        typeInputs.forEach(input => {
            input.classList.toggle("is-invalid", Boolean(message));
            input.setAttribute("aria-invalid", message ? "true" : "false");
        });
    }

    function setContentError(message) {
        contentError.textContent = message || "";
        contentInput.classList.toggle("is-invalid", Boolean(message));
        contentInput.setAttribute("aria-invalid", message ? "true" : "false");
    }

    function validateType(showErrors) {
        const type = selectedType();
        const valid = type === 1 || type === 2;
        if (showErrors) {
            setTypeError(valid ? "" : "Choose Problem or Suggestion.");
        }
        return valid;
    }

    function validateContent(showErrors) {
        const length = trimmedContent().length;
        let message = "";

        if (length === 0) {
            message = "Enter a message.";
        } else if (length < MIN_LENGTH) {
            message = `The message must be at least ${MIN_LENGTH} characters.`;
        } else if (length > MAX_LENGTH) {
            message = `The message must be ${MAX_LENGTH} characters or fewer.`;
        }

        if (showErrors) {
            setContentError(message);
        }
        return message === "";
    }

    function validateAll(showErrors) {
        // Validate both fields before returning so both messages appear together.
        const typeValid = validateType(showErrors);
        const contentValid = validateContent(showErrors);
        return typeValid && contentValid;
    }

    function clearStatus() {
        status.textContent = "";
        status.className = "mb-3";
    }

    function showSuccess(referenceNumber, message) {
        const alert = document.createElement("div");
        alert.className = "alert alert-success";
        alert.setAttribute("role", "status");

        const title = document.createElement("p");
        title.className = "mb-1 fw-semibold";
        title.textContent = message || "Thank you! Your feedback was received.";
        alert.appendChild(title);

        if (referenceNumber) {
            const reference = document.createElement("p");
            reference.className = "mb-0";
            reference.textContent = `Reference number: ${referenceNumber}`;
            alert.appendChild(reference);
        }

        status.className = "mb-3";
        status.appendChild(alert);
    }

    function showError(message) {
        const alert = document.createElement("div");
        alert.className = "alert alert-danger";
        alert.setAttribute("role", "alert");
        alert.textContent = message;

        status.className = "mb-3";
        status.appendChild(alert);
    }

    function firstServerError(errors, propertyName) {
        if (!errors) {
            return null;
        }

        const key = Object.keys(errors).find(name => name.toLowerCase() === propertyName.toLowerCase());
        const messages = key ? errors[key] : null;
        return Array.isArray(messages) && messages.length > 0 ? messages[0] : null;
    }

    function focusFirstInvalid() {
        const type = selectedType();
        if (type !== 1 && type !== 2) {
            if (typeInputs[0]) {
                typeInputs[0].focus();
            }
            return;
        }

        contentInput.focus();
    }

    function setFieldsDisabled(disabled) {
        typeInputs.forEach(input => {
            input.disabled = disabled;
        });
        contentInput.disabled = disabled;
    }

    async function submitFeedback() {
        clearStatus();

        typeTouched = true;
        contentTouched = true;

        if (!validateAll(true)) {
            focusFirstInvalid();
            updateSubmitButton();
            return;
        }

        isSubmitting = true;
        setFieldsDisabled(true);
        form.setAttribute("aria-busy", "true");
        updateSubmitButton();

        try {
            const response = await fetch(API_URL, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    type: selectedType(),
                    content: trimmedContent()
                })
            });

            if (response.ok) {
                const data = await response.json().catch(() => null);

                // Never claim success without the incoming number: a login redirect
                // or a proxy error page can also answer with HTTP 200.
                if (!data || !data.referenceNumber) {
                    showError("We couldn't confirm your feedback was saved. Please try again.");
                    return;
                }

                showSuccess(data.referenceNumber, data.message);

                form.reset();
                typeTouched = false;
                contentTouched = false;
                setTypeError("");
                setContentError("");
                updateCounter();
                return;
            }

            const problem = await response.json().catch(() => null);
            const typeMessage = firstServerError(problem && problem.errors, "Type");
            const contentMessage = firstServerError(problem && problem.errors, "Content");

            if (typeMessage) {
                typeTouched = true;
                setTypeError(typeMessage);
            }
            if (contentMessage) {
                contentTouched = true;
                setContentError(contentMessage);
            }

            if (typeMessage || contentMessage) {
                focusFirstInvalid();
            } else {
                showError("Something went wrong. Please try again.");
            }
        } catch (error) {
            // Keep whatever the user typed: they should not have to write it again.
            showError("Could not reach the server. Check your connection and try again.");
        } finally {
            isSubmitting = false;
            setFieldsDisabled(false);
            form.removeAttribute("aria-busy");
            updateSubmitButton();
        }
    }

    typeInputs.forEach(input => {
        input.addEventListener("change", () => {
            typeTouched = true;
            clearStatus();
            validateType(true);
            updateSubmitButton();
        });

        input.addEventListener("blur", () => {
            if (typeTouched) {
                validateType(true);
            }
        });
    });

    contentInput.addEventListener("input", () => {
        clearStatus();
        updateCounter();

        if (contentTouched) {
            validateContent(true);
        }
        updateSubmitButton();
    });

    contentInput.addEventListener("blur", () => {
        contentTouched = true;
        validateContent(true);
        updateSubmitButton();
    });

    form.addEventListener("submit", event => {
        event.preventDefault();

        if (isSubmitting) {
            return;
        }

        submitFeedback();
    });

    // Browsers can restore the page from the back/forward cache mid-request.
    window.addEventListener("pageshow", () => {
        isSubmitting = false;
        setFieldsDisabled(false);
        form.removeAttribute("aria-busy");
        updateSubmitButton();
    });

    updateCounter();
    updateSubmitButton();
})();
