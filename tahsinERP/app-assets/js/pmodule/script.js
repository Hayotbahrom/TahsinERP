$(document).ready(function () {
    $('#controllerDropdown').change(function () {
        var controllerName = $(this).val();
        if (controllerName !== '') {
            $.ajax({
                url: '@Url.Action("GetActions", "PModule")',
                type: 'POST',
                data: { controller: controllerName },
                success: function (data) {
                    var actionDropdown = $('#actionDropdown');
                    actionDropdown.empty();
                    actionDropdown.append($('<option>', {
                        value: '',
                        text: 'Tanlang'
                    }));
                    $.each(data, function (index, item) {
                        actionDropdown.append($('<option>', {
                            value: item,
                            text: item
                        }));
                    });
                },
                error: function () {
                    console.error('Amallar olishda xato roʻy berdi.');
                }
            });
        }
    });

    $('#createForm').submit(function (e) {
        e.preventDefault();
        var form = $(this);
        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            success: function (response) {
                // Handle success response here
                window.location.href = response.redirectUrl; // Example: redirect to success page
            },
            error: function () {
                console.error('Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib koʻring va agar muammo davom etsa, tizim administratoriga murojaat qiling.');
            }
        });
    });
});