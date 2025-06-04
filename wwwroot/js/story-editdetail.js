// story-editdetail.js

document.addEventListener("DOMContentLoaded", function () {
    // Toggle inline add chapter form
    const formContainer = document.getElementById("inlineChapterForm");
    const form = document.querySelector("#createChapterForm");
    const toggleButton = document.getElementById("toggleAddChapter");
    const cancelButton = document.getElementById("cancelAddChapter");

    if (toggleButton && formContainer) {
        toggleButton.addEventListener("click", () => {
            formContainer.style.display = "table-row";
            toggleButton.style.display = "none";
        });
    }
    if (cancelButton && formContainer && toggleButton) {
        cancelButton.addEventListener("click", () => {
            form.reset();
            formContainer.style.display = "none";
            toggleButton.style.display = "inline-block";
        });
    }

    // Chapter order edit logic
    document.querySelectorAll('.edit-order-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const tr = btn.closest('tr');
            const orderForm = tr.querySelector('.order-form');
            orderForm.style.display = 'inline-flex';
            btn.style.display = 'none';
        });
    });
    document.querySelectorAll('.cancel-order-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const tr = btn.closest('tr');
            const orderForm = tr.querySelector('.order-form');
            const editBtn = tr.querySelector('.edit-order-btn');
            orderForm.style.display = 'none';
            if (editBtn) editBtn.style.display = 'inline-block';
        });
    });
});
