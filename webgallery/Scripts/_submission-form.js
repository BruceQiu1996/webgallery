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

function setExplanationPosition(textContainer, explanationPanel)
{
    var left = textContainer.offsetLeft + textContainer.offsetWidth + 20;
    var top = textContainer.offsetTop + (textContainer.offsetHeight / 2) - 28;
    explanationPanel.css({ visibility: "visible", left: left + "px", top: top + "px" });
}

function addHandlersForTab(tabContainer)
{
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

function characterCountDown(maximum, textContainer)
{
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

function createNickname(appNameContainer)
{
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
$("form").validate({
    invalidHandler: function (e, validator) {
        var errors = validator.numberOfInvalids();
        if (errors)
            $("#validationEntriesPanel").css("display", "block");
    },
    rules: {
        AppId: {
            required: true,
            alphanumeric: true,
            remote: {
                url: "/Manage/ValidateAppIdVersion",
                type: "post",
                data: {
                    appId: function () { return $("#AppId").val().trim(); },
                    version: function () { return $("#Version").val().trim(); },
                    submissionId: function () { return $("#SubmissionId").val().trim(); }
                }
            }
        },
        Version: "required",
        SubmittingEntity: "required",
        SubmittingEntityURL: { required: true, url: true },
        AppWebSiteURL: { required: true, url: true },
        SupportURL: { required: true, url: true },
        ReleaseDate: { required: true },
        PrimaryCategory: "required",
        ProfessionalServicesURL: { url: true },
        CommercialProductURL: { url: true }
    },
    errorElement: "span",
    errorClass: "jqueryvalidation-error",
    onkeyup: false,
    debug: true,
    errorPlacement: function (error, element) {
        error.addClass("validator-below");
        error.insertAfter(element);

        // add to Validation Entries
        var dataTitle = element.attr("data-title");
        var elementId = element.attr("id");
        var a = $("<a></a>").html(dataTitle + ": " + error.text())
                    .attr("href", "#" + elementId);
        var li = a.wrap("<li class='jqueryvalidation-error'></li>").parent();
        li.attr("for", elementId);
        li.appendTo("#validationEntriesPanel ul");
    },
    showErrors: function (errorMap, errorList) {
        this.defaultShowErrors();

        // refresh Validation Entries
        var invlidElements = this.invalid;
        $("#validationEntriesPanel ul li").each(function (i, liElement) {
            var elementId = $(liElement).attr("for");
            if (!invlidElements[elementId]) {
                $(liElement).remove();
            }
        });
    }
});