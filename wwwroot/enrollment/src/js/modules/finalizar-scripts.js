(() => {
    "use strict";

    function initWizardSteps() {
        const steps = document.querySelectorAll("#wizardSteps .step-vertical");
        if (!steps.length) return;

        steps.forEach((el) => {
            const i = Number(el.dataset.step);
            el.classList.remove("active", "complete");

            if (i < 2) el.classList.add("complete");
            if (i === 2) el.classList.add("active");
        });
    }

    document.addEventListener("DOMContentLoaded", () => {
        initWizardSteps();
    });
})();