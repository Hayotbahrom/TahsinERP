document.addEventListener('DOMContentLoaded', () => {
    const popup = document.getElementById('pop-up');
    const openButtons = document.querySelectorAll('.open-modal');
    const closeButton = document.getElementById('pop-up__close');
    const confirmButton = document.getElementById('pop-up__action');
    let formToSubmit = null;

    function openPopup() {
        popup.classList.add('show');
    }

    function closePopup() {
        popup.classList.remove('show');
    }

    openButtons.forEach(button => {
        button.addEventListener('click', (event) => {
            event.preventDefault();
            formToSubmit = button.closest('form');
            openPopup();
        });
    });

    closeButton.addEventListener('click', closePopup);
    confirmButton.addEventListener('click', () => {
        if (formToSubmit) {
            formToSubmit.submit();
        }
        closePopup();
    });
});
