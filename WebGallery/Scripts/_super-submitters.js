$(document).ready(function () {
    $("#add-submitter").validate({
        onfocusout: function (element) {
            this.element(element);
        },
        rules: {
            "microsoftAccount": {
                required: true,
                email: true
            },
            "confirmation": {
                equalTo: "#microsoftAccount"
            }
        },
        messages: {
            "microsoftAccount": {
                required: "*",
                email: "unknown eMail format"
            },
            "confirmation": "unmatched eMail address"
        },
        onkeyup: false
    });
    $("#add-submitter").find("input").blur(function () {
        if ($("#add-submitter").valid()) {
            $(this).parent().parent().children().last().prev().find("input").removeAttr("disabled");
        }
    });
})