$(function () {
    addHandlersForTab($("#metaDataTabContainer"));
    addHandlersForTab($("#outerPackageTabContainer"));
    $("#appSubmitContainer input:text,textarea,select,input:file").bind("mouseenter mouseleave", function () { $(this).toggleClass("input-mouseover"); });
    $("#appSubmitContainer input:text,textarea,select").focusin(function () { setExplanationPosition(this, $(this).nextAll(".explanation:first")); });
    $("#appSubmitContainer input:text,textarea,select").focusout(function () { $(this).nextAll(".explanation:first").css({ visibility: "hidden" }); });
    $("#appSubmitContainer input:file").focusin(function () { setExplanationPosition(this, $(this).parent().nextAll(".explanation:first")); });
    $("#appSubmitContainer input:file").focusout(function () { $(this).parent().nextAll(".explanation:first").css({ visibility: "hidden" }); });

    $(".low-count").each(function () {
        var textContainer = $(this).parent().parent().prev();
        var maxLength = $(this).html();
        characterCountDown(maxLength, textContainer);
    });

    // add date picker
    $("#appSubmitContainer .datefield").datepicker({
        todayBtn: "linked"
    });
});

function setExplanationPosition(textContainer, explanationPanel) {
    var left = textContainer.offsetLeft + textContainer.offsetWidth + 20;
    var top = textContainer.offsetTop + (textContainer.offsetHeight / 2) - 28;
    explanationPanel.css({ visibility: "visible", left: left + "px", top: top + "px" });
}

function addHandlersForTab(tabContainer) {
    $(tabContainer).children(":first").children().each(function (i, tabHeader) {
        $(tabHeader).bind("mouseenter mouseleave", function () {
            $(this).toggleClass("tab_hover");
        });

        $(tabHeader).click(function () {
            $(this).siblings().removeClass();
            $(this).addClass("tab_active");

            var language = $(this).attr("data-language");
            $(this).parent().next().children().each(function (j, tabBody) {
                var idStr = $(tabBody).attr("id");
                if (idStr.indexOf(language) != -1) {
                    $(tabBody).css({ display: "block" });
                }
                else {
                    $(tabBody).css({ display: "none" });
                }
            });
        });
    });
}

function characterCountDown(maximum, textContainer) {
    var countdownElement = $(textContainer).next(".explanation").find("span");
    if (countdownElement.length != 1) return;

    countdownElement.removeClass();

    var remaining = maximum - $(textContainer).val().length;
    if (remaining > 10) {
        countdownElement.addClass("low-count");
    }
    else if (remaining > -1) {
        countdownElement.addClass("high-count");
    }
    else {
        countdownElement.addClass("exceeded-count");
    }

    countdownElement.html(remaining);

    // update form validator
}

function createNickname(appNameContainer) {
    // We don't have any value yet for the AppID (aka. Nickname). We can suggest a value
    // by deriving a well formatted AppID from the AppName.

    var appId = $("#AppId").val();
    if (appId.trim().length == 0) // if appId is empty or whitespaces
    {
        var re = new RegExp("\\W", "g");
        var appName = $(appNameContainer).val();
        var suggestedAppId = appName.replace(re, "");
        $("#appId").val(suggestedAppId);
    }
}

$.validator.addMethod("alphanumeric", function (value, element) {
    return /^\w*$/.test(value);
});
var uniqueAppIdAndVersion = {
    url: "/Manage/ValidateAppIdVersion",
    type: "post",
    data: {
        appId: function () { return $("#AppId").val().trim(); },
        version: function () { return $("#Version").val().trim(); },
        submissionId: function () { return $("#SubmissionId").val().trim(); }
    }
};
$("form").validate({
    debug: true,
    rules: {
        AppId: {
            required: true,
            alphanumeric: true,
            remote: uniqueAppIdAndVersion
        },
        Version: {
            required: true
        },
        SubmittingEntity: "required",
        SubmittingEntityURL: { required: true, url: true },
        AppWebSiteURL: { required: true, url: true },
        SupportURL: { required: true, url: true },
        ReleaseDate: { required: true },
        PrimaryCategory: "required",
        ProfessionalServicesURL: { url: true },
        CommercialProductURL: { url: true }
    },
    errorContainer: "#validationEntriesContainer",
    errorElement: "span",
    errorClass: "jqueryvalidation-error",
    onkeyup: false,
    onfocusout: function (element, event) {
        $(element).valid();

        // when validating Version, 
        // we also need trigger AppId validation to see if the combination of they two is unique
        if ($(element).attr("id") == "Version" && $(element).val().length > 0) {
            $("#AppId").valid();
        }
    },
    errorPlacement: function (error, element) {
        error.addClass("validator-below");
        error.insertAfter(element);
    },
    showErrors: function (errorMap, errorList) {
        this.defaultShowErrors();

        //
        // display errors in Validation Entries
        //
        var invalidElements = this.invalid;
        $("#validationEntriesContainer ul li").each(function (index, li) {
            var forVal = $(li).attr("for");
            if (!invalidElements[forVal]) {
                $(li).remove(); // remove the entries that pass the validation
            }
        });

        // create/update error entry for each of invalid elements
        for (var elementId in this.invalid) {
            showErrorInValidationEntires(errorMap[elementId], elementId);
        }
    }
});

function showErrorInValidationEntires(errorMessage, elementId) {
    var dataTitle = $("#" + elementId).attr("data-title");

    // try to find the error entry associated with the error element
    var liQuery = $("#validationEntriesContainer ul").find("[for='" + elementId + "']");
    if (liQuery.length == 0) { // if not found, create a new one
        var a = $("<a></a>").html(dataTitle + ": " + errorMessage).attr("href", "#" + elementId);
        var li = a.wrap("<li class='jqueryvalidation-error' data-title='" + dataTitle + "'></li>").parent();
        li.attr("for", elementId);
        li.appendTo("#validationEntriesContainer ul");
    }
    else { // if found,
        // and if the error message is not undefined, then udpate the entry
        if (errorMessage != undefined) {
            var a = liQuery.find("a").html(dataTitle + ": " + errorMessage);
        }
    }
}