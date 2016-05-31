$(document).ready(function () {
    $.validator.methods.required = function (value, element, param) {
        console.log("required", element);
        return $.trim($(element).val()) != "" && value != null;
    }

    //AddValidationMethod:Validation for Country
    jQuery.validator.addMethod("ValiCountry", function (value, element) {
        return $("#ContactDetail_Country option:selected").val() != "0";
    }, "");

    //AddValidationMethod:Validation for State,if one choose USA as Country
    jQuery.validator.addMethod("ValiState", function (value, element) {
        if ($("#ContactDetail_Country option:selected").val() == "USA") {
            return $("#State option:selected").val() != "" && $("#State option:selected").val() != null;
        }
        else return true;
    }, "");

    //AddValidationMethod:If one choose USA as Country ,he must fill the ZIP or Region Code
    jQuery.validator.addMethod("ValiZip", function (value, element) {
        if ($("#ContactDetail_Country option:selected").val() == "USA") {
            return $.trim(String(value)) != "";
        }
        else return true;
    }, "");

    //Form Validation
    $('#publisherdetailsform').validate({
        onfocusout: function (element) {
            this.element(element);
        },
        rules: {
            "ContactDetail.FirstName": {
                required: true,
            },
            "ContactDetail.LastName": {
                required: true,
            },
            "ContactDetail.City": {
                required: true,
            },
            "ContactDetail.Address1": {
                required: true,
            },
            "ContactDetail.EMail": {
                required: true,
                email: true,
            },
            "ContactDetail.Country": {
                ValiCountry: true
            },
            State: {
                ValiState: true
            },
            "ContactDetail.ZipOrRegionCode": {
                ValiZip: true
            }
        },
        messages: {
            "ContactDetail.FirstName": "*",
            "ContactDetail.LastName": "*",
            "ContactDetail.City": "*",
            "ContactDetail.Address1": "*",
            "ContactDetail.EMail": {
                required: "*",
                email: "unknown eMail format",
            },
            "ContactDetail.Country": "*",
            State: "*",
            "ContactDetail.ZipOrRegionCode": "*"
        },
        onkeyup: false
    })

    //If one choose USA as Country ,State will appear and Province will disappear, or opposed;and form will be valided while state and country selected changed
    $("#ContactDetail_Country").change(function () {
        if ($("#ContactDetail_Country option:selected").val() == "USA") {
            $("#ContactDetail_StateOrProvince").hide();
            $("#LBProvince").hide();
            $("#State").show();
            $("#LBState").show();
        }
        else {
            $("#ContactDetail_StateOrProvince").show();
            $("#LBProvince").show();
            $("#State").hide();
            $("#LBState").hide();
            $("#ContactDetail_StateOrProvince").val("");
        }
        $("#publisherdetailsform").valid();
    }).ready(function () {
        if ($("#ContactDetail_Country option:selected").val() == "USA") {
            $("#ContactDetail_StateOrProvince").hide();
            $("#LBProvince").hide();
            $("#State").show();
            $("#LBState").show();
        }
        else {
            $("#ContactDetail_StateOrProvince").show();
            $("#LBProvince").show();
            $("#State").hide();
            $("#LBState").hide();
        }
        $("#publisherdetailsform").valid();
    });
    $("#State").change(function () {
        $("#publisherdetailsform").valid();
    }).ready(function () {
        if ($("#ContactDetail_Country option:selected").val() == "USA")
            $("#State").val($("#ContactDetail_StateOrProvince").val());
        $("#publisherdetailsform").valid();
    })
    $("#submit").click(function () {
        if ($("#ContactDetail_Country option:selected").val() == "USA")
            $("#ContactDetail_StateOrProvince").val($("#State").val());
    })
});






