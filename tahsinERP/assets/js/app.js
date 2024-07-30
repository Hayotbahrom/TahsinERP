document.addEventListener('DOMContentLoaded', () => {
    const popup = document.getElementById('popup');
    const openButtons = document.querySelectorAll('.open-modal');
    const closeButton = document.getElementById('popup-close');
    const popupForm = document.getElementById('popup-form');
    let functionToCall = null;

    function openPopup() {
        popup.classList.add('show');
    }

    function closePopup() {
        popup.classList.remove('show');
    }



    openButtons.forEach(button => {
        button.addEventListener('click', (event) => {
            event.preventDefault();
            functionToCall = button.getAttribute('data-function');
            openPopup();
        });
    });

    closeButton.addEventListener('click', closePopup);
    popupForm.addEventListener('submit', executeFunction);
});
