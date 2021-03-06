﻿$(document).ready(function () {
    // bind mouseenter/mouseleave/click handlers for metadata and package tabs
    addHandlersForTab($("#metaDataTabContainer"));
    addHandlersForTab($("#packageTabContainer"));

    // toggle a CSS class when mouse enter or leave those inputs
    $("#appSubmitContainer input:text,textarea,select,input:file").bind("mouseenter mouseleave", function () { $(this).toggleClass("input-mouseover"); });

    // for explanation panels, show/hide when focus in/out
    $("#appSubmitContainer input:text,textarea,select,input:file").focusin(function () { showExplanation(this, $(this).nextAll(".explanation:first")); });
    $("#appSubmitContainer input:text,textarea,select,input:file").focusout(function () { $(this).nextAll(".explanation:first").hide(); });

    // add date picker
    addDatepicker("#appSubmitContainer .datefield");

    // bind warning changes logic for those textboxes in packageTabContainer
    $("#packageTabContainer :text").change(function () { warnIfPackageInfoChanges(); });
    $("#packageTabContainer :button").click(function () { warnIfPackageInfoChanges(); }); // clicking Copy/Clear buttons will change package info

    // bind validating logic for those inputs
    $("#appSubmitContainer :text,#appSubmitContainer textarea").blur(function () { validate(); });
    $("#PrimaryCategory,#FrameworksAndRuntimes,#AcceptTermsAndConditions,#logo").change(function () { validate(); });
    $("#packageTabContainer :button").click(function () { validate(); });
    $("#clearDependencies,#resetLogo,#replaceLogo").click(function () { validate(); });

    // do a validating after page loading
    validate();
});

function showExplanation(textContainer, explanationPanel) {
    var left = textContainer.offsetLeft + textContainer.offsetWidth + 20;
    var top = textContainer.offsetTop + (textContainer.offsetHeight / 2) - 28;
    explanationPanel.css({ left: left + "px", top: top + "px" });
    explanationPanel.show();
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
                    $(tabBody).show();
                }
                else {
                    $(tabBody).hide();
                }
            });
        });
    });
}

function characterCountDown(maximum, textContainer) {
    var countdownElement = $(textContainer).next(".explanation").find("strong");
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
        $("#AppId").val(suggestedAppId);
    }
}

//
// validation logic
//
function validate() {
    var errors = $([]);

    validateMetadata(errors);
    validateDescription(errors);
    validateBriefDescription(errors);
    validateAppId(errors);
    validateVersion(errors);
    validateSubmittingEntity(errors);
    validateSubmittingEntityURL(errors);
    validateAppWebSiteURL(errors);
    validateSupportURL(errors);
    validateReleaseDate(errors);
    validatePrimaryCategory(errors);
    validateProfessionalServicesURL(errors);
    validateCommercialProductURL(errors);
    validateLogo(errors);
    validatePackageInfo(errors);
    validatePackageLocationUrl(errors);
    validateDependencies(errors);
    validateTermsAndConditions(errors);

    showErrors(errors);

    return errors.length == 0;
}

function showErrors(errors) {
    $("#appSubmitContainer .appgallery-validation-error").remove(); // remove inline validators
    $("#validationEntriesContainer ul").empty(); // empty Validation Entries 

    if (errors.length > 0)
        $("#validationEntriesContainer").show();
    else
        $("#validationEntriesContainer").hide();

    errors.each(function (i, error) {
        var target = $("#" + error.id);

        // display inline errors
        var errorMessage = target.attr("data-msg-" + error.type);
        var errorClass = (errorMessage == "*" || errorMessage == "(*)") ? "validator-inline" : "validator-below";
        var errorElement = $("<span class='appgallery-validation-error'></span>").html(errorMessage).addClass(errorClass);
        var errorTarget = target.attr("data-target");
        errorElement.insertAfter(errorTarget ? $("#" + errorTarget) : target);

        //
        // display errors in Validation Entries
        //
        var detailedErrorMessage = target.attr("data-msg-d-" + error.type)
        errorMessage = detailedErrorMessage ? detailedErrorMessage : errorMessage
        var dataTitle = target.attr("data-title");
        var htmlContent = dataTitle ? (dataTitle + ": " + errorMessage) : errorMessage;

        var anchor = target.attr("data-anchor");
        var href = anchor ? anchor : error.id;

        var a = $("<a></a>").html(htmlContent).attr("href", "#" + href);
        var li = a.wrap("<li class='appgallery-validation-error'></li>").parent();
        li.appendTo("#validationEntriesContainer ul");
    });
}

// for metadata
function validateMetadata(errors) {
    var metadataList = getMetadataList();

    if (!englishMetadataRequired(metadataList)) {
        errors.push({ id: "DescribeYourApplication", type: "englishMetadataRequired" });
        return;
    }

    if (!eachPackageHasMetadataForItsLanguage(metadataList, getPackageList())) {
        errors.push({ id: "DescribeYourApplication", type: "eachPackageHasMetadataForItsLanguage" });
        return;
    }
}

function englishMetadataRequired(metadataList) {
    var englishMetadata = null;
    metadataList.each(function (i, metadata) {
        if (metadata.language.toLowerCase() == "en-us") {
            englishMetadata = metadata;
            return false;
        }
    });

    return englishMetadata.isCompleted();
}

function eachPackageHasMetadataForItsLanguage(metadataList, packageList) {
    var pass = true;
    packageList.each(function (i, p) {
        metadataList.each(function (j, metadata) {
            if (p.language == metadata.language && p.isCompleted() && !metadata.isCompleted()) {
                pass = false;
                return false;
            }
        });
    });

    return pass;
}

function getMetadataList() {
    var metadataList = $([]);
    var metadataTabs = $("#metaDataTabContainer .tab_body").children();
    metadataTabs.each(function (i, tab) {
        metadataList.push(getMetadata($(tab)));
    });

    return metadataList;
}

function getMetadata(metadataTab) {
    var language = metadataTab.find("input[name$='.Language']").val();
    var appName = metadataTab.find("input[name$='.Name']").val().trim();
    var description = metadataTab.find("textarea[name$='.Description']").val().trim();
    var briefDescription = metadataTab.find("textarea[name$='.BriefDescription']").val().trim();

    return {
        language: language,
        appName: appName,
        description: description,
        briefDescription: briefDescription,
        isCompleted: function () {
            return isMetadataCompleted(this);
        }
    };
}

function isMetadataCompleted(metadata) {
    return metadata.appName != "" && metadata.description != "" && metadata.briefDescription != "";
}

// for Description
function validateDescription(errors) {
    var descriptions = $("#metaDataTabContainer textarea[name^='Description']");

    descriptions.each(function (i, e) {
        if ($(e).val().length > 1500) {
            errors.push({ id: $(e).attr("id"), type: "maxlength" });
        }
    });
}

// for Brief Description
function validateBriefDescription(errors) {
    var bds = $("#metaDataTabContainer textarea[name^='BriefDescription']");

    bds.each(function (i, e) {
        if ($(e).val().length > 400) {
            errors.push({ id: $(e).attr("id"), type: "maxlength" });
        }
    });
}

// for App Id
function validateAppId(errors) {
    var appId = $("#AppId").val();

    if (appId.trim().length == 0) {
        errors.push({ id: "AppId", type: "required" });
        return;
    }

    if (! /^\w*$/.test(appId)) {
        errors.push({ id: "AppId", type: "alphanumeric" });
        return;
    }

    var uniqueAppIdAndVersion = {
        url: "/app/nickname/version/validate",
        type: "post",
        data: {
            appId: function () { return $("#AppId").val().trim(); },
            version: function () { return $("#Version").val().trim(); },
            submissionId: function () { return $("#SubmissionId").val().trim(); }
        }
    };

    $.ajax(uniqueAppIdAndVersion).done(function (isUnique) {
        if (!isUnique) {
            errors.push({ id: "AppId", type: "unique" });
            showErrors(errors);
        }
    });
}

// for Version
function validateVersion(errors) {
    var version = $("#Version").val();

    if (version.trim().length == 0) {
        errors.push({ id: "Version", type: "required" });
        return;
    }
}

// for Submitting Entity
function validateSubmittingEntity(errors) {
    var val = $("#SubmittingEntity").val();

    if (val.trim().length == 0) {
        errors.push({ id: "SubmittingEntity", type: "required" });
        return;
    }
}

// for Submitting Entity URL
function validateSubmittingEntityURL(errors) {
    var val = $("#SubmittingEntityURL").val();

    if (val.trim().length == 0) {
        errors.push({ id: "SubmittingEntityURL", type: "required" });
        return;
    }

    if (!validateURL(val)) {
        errors.push({ id: "SubmittingEntityURL", type: "url" });
        return;
    }
}

// for App Web Site URL
function validateAppWebSiteURL(errors) {
    var val = $("#AppWebSiteURL").val();

    if (val.trim().length == 0) {
        errors.push({ id: "AppWebSiteURL", type: "required" });
        return;
    }

    if (!validateURL(val)) {
        errors.push({ id: "AppWebSiteURL", type: "url" });
        return;
    }
}

// for Support URL
function validateSupportURL(errors) {
    var val = $("#SupportURL").val();

    if (val.trim().length == 0) {
        errors.push({ id: "SupportURL", type: "required" });
        return;
    }

    if (!validateURL(val)) {
        errors.push({ id: "SupportURL", type: "url" });
        return;
    }
}

// for Release Date
function validateReleaseDate(errors) {
    var val = $("#ReleaseDate").val();

    if (val.trim().length == 0) {
        errors.push({ id: "ReleaseDate", type: "required" });
        return;
    }
}

// for Primary Category
function validatePrimaryCategory(errors) {
    var val = $("#PrimaryCategory").val();

    if (val.trim().length == 0) {
        errors.push({ id: "PrimaryCategory", type: "required" });
        return;
    }
}

// for Professional Services URL
function validateProfessionalServicesURL(errors) {
    var val = $("#ProfessionalServicesURL").val();

    if (val.trim().length > 0 && !validateURL(val)) {
        errors.push({ id: "ProfessionalServicesURL", type: "url" });
        return;
    }
}

// for Commercial Product URL
function validateCommercialProductURL(errors) {
    var val = $("#CommercialProductURL").val();

    if (val.trim().length > 0 && !validateURL(val)) {
        errors.push({ id: "CommercialProductURL", type: "url" });
        return;
    }
}

// for Logo
function validateLogo(errors) {
    var setLogo = $("#SetLogo").val() == "True";
    if (setLogo && $("#logo").get(0).files.length == 0) {
        errors.push({ id: "logo", type: "required" });
        return;
    }
}

// for package info
function validatePackageInfo(errors) {
    var packageList = getPackageList();

    if (!atLeastOnePackageCompleted(packageList)) {
        errors.push({ id: "ProvidePackageInformation", type: "atLeastOnePackageCompleted" });
        return;
    }

    if (!eachPackageInfoCompleted(packageList)) {
        errors.push({ id: "ProvidePackageInformation", type: "eachPackageInfoCompleted" });
        return;
    }
}

function atLeastOnePackageCompleted(packageList) {
    var pass = false;
    packageList.each(function (i, p) {
        if (p.isCompleted()) {
            pass = true;
            return false;
        }
    });

    return pass;
}

function eachPackageInfoCompleted(packageList) {
    var pass = true;
    packageList.each(function (i, p) {
        if (p.hasInput() && !p.isCompleted()) {
            pass = false;
            return false;
        }
    });

    return pass;
}

function getPackageList() {
    var packageList = $([]);
    var packageTabs = $("#packageTabContainer .tab_body").children();
    packageTabs.each(function (i, tab) {
        packageList.push(getPackage($(tab)));
    });

    return packageList;
}

function getPackage(packageTab) {
    var language = packageTab.find("input:hidden[name$='.Language']").val();
    var packageLocationUrl = packageTab.find("input[name$='.PackageUrl']").val().trim();
    var startPage = packageTab.find("input[name$='.StartPage']").val().trim();
    var sha1Hash = packageTab.find("input[name$='.SHA1Hash']").val().trim();

    return {
        language: language,
        packageLocationUrl: packageLocationUrl,
        startPage: startPage,
        sha1Hash: sha1Hash,
        isCompleted: function () {
            return isPackageCompleted(this);
        },
        hasInput: function () {
            return pagecakgeHasInput(this);
        }
    };
}

function isPackageCompleted(p) {
    return p.packageLocationUrl != "" && p.sha1Hash != "";
}

function pagecakgeHasInput(p) {
    return p.packageLocationUrl != "" || p.sha1Hash != "" || p.startPage != "";
}

// for Package Location Url
function validatePackageLocationUrl(errors) {
    var urls = $("#packageTabContainer input[name$='.PackageUrl']");

    urls.each(function (i, u) {
        if ($(u).val().trim().length > 0 && !validateURL($(u).val())) {
            errors.push({ id: $(u).attr("id"), type: "url" });
        }
    });
}

// for Dependencies
function validateDependencies(errors) {
    var selectedIndex = $("#FrameworksAndRuntimes").prop("selectedIndex");

    if (selectedIndex == -1) {
        errors.push({ id: "Dependencies", type: "required" });
        return;
    }
}

// for Terms And Conditions
function validateTermsAndConditions(errors) {
    var checked = $("#AcceptTermsAndConditions").get(0).checked;

    if (!checked) {
        errors.push({ id: "AcceptTermsAndConditions", type: "required" });
        return;
    }
}

//
// common functions
//

function validateURL(url) {
    // This regular expression is a variation of the one found at http://regexlib.com/REDetails.aspx?regexp_id=1121.
    // Here, we've modified it so the protocol (e.g., http://) is not required.
    var re = new RegExp("^((([hH][tT][tT][pP][sS]?|[fF][tT][pP])\\:\\/\\/){0,1}([\\w\\.\\-]+(\\:[\\w\\.\\&%\\$\\-]+)*@)?((([^\\s\\(\\)\\<\\>\\\\\\\"\\.\\[\\]\\,@;:]+)(\\.[^\\s\\(\\)\\<\\>\\\\\\\"\\.\\[\\]\\,@;:]+)*(\\.[a-zA-Z]{2,4}))|((([01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\.){3}([01]?\\d{1,2}|2[0-4]\\d|25[0-5])))(\\b\\:(6553[0-5]|655[0-2]\\d|65[0-4]\\d{2}|6[0-4]\\d{3}|[1-5]\\d{4}|[1-9]\\d{0,3}|0)\\b)?((\\/[^\\/][\\w\\.\\,\\?\\'\\\\\\/\\+&%\\$#\\=~_\\-@]*)*[^\\.\\,\\?\\\"\\'\\(\\)\\[\\]!;<>{}\\s\\x7F-\\xFF])?)$", "g");
    return re.test(url);
}

//
// for package values
//
function copyValues(sender) {
    var lang = $(sender).next().val() + "_x86";
    var sourcePackageLocationUrl = $("#PackageLocationUrl_" + lang).val();
    var sourceStartPage = $("#StartPage_" + lang).val();
    var sourceSha1Hash = $("#Sha1Hash_" + lang).val();

    $(sender).siblings("input[name$='.PackageUrl']").val(sourcePackageLocationUrl);
    $(sender).siblings("input[name$='.StartPage']").val(sourceStartPage);
    $(sender).siblings("input[name$='.SHA1Hash']").val(sourceSha1Hash);
}

function clearValues(sender) {
    $(sender).siblings("input[name$='.PackageUrl']").val("");
    $(sender).siblings("input[name$='.StartPage']").val("");
    $(sender).siblings("input[name$='.SHA1Hash']").val("");
}

//
// for dependencies
//
function clearDependencies() {
    $("#FrameworksAndRuntimes").prop("selectedIndex", -1);
    $("#DatabaseServers").prop("selectedIndex", -1);
    $("#WebServerExtensions").prop("selectedIndex", -1);
}

function autoSelectMicrosoftSqlDriverForPhp() {
    // If the Framework/Runtime is PHP and the Database is SQL Server (including Express) then we also need (as a dependency)
    // Microsoft SQL Driver for PHP. Otherwise, Microsoft SQL Driver for PHP is not a necessary dependency. 

    // Start by de-selecting the dependency Microsoft SQL Driver for PHP. Then, if the framework is PHP and the DB is SQL Server, select it.
    deselectMicrosoftSqlDriverForPhp();

    if (isPhpSelected() && isSqlServerSelected()) {
        selectMicrosoftSqlDriverForPhp();
    }
}

function deselectMicrosoftSqlDriverForPhp() {
    $("#DatabaseServers option").each(function (i, e) {
        if (e.text.toLowerCase().indexOf("microsoft sql driver for php") == 0) {
            e.selected = false;
        }
    });
}

function isPhpSelected() {
    var selectedFramework = $("#FrameworksAndRuntimes option:selected");
    return selectedFramework.text().toLowerCase().indexOf("php") == 0;
}

function isSqlServerSelected() {
    var selected = false;
    $("#DatabaseServers option").each(function (i, e) {
        if (e.selected && e.text.toLowerCase().indexOf("sql server") == 0) {
            selected = true;
            return false; // if selected, then break out the loop immediately
        }
    });

    return selected;
}

function selectMicrosoftSqlDriverForPhp() {
    $("#DatabaseServers option").each(function (i, e) {
        if (e.text.toLowerCase().indexOf("microsoft sql driver for php") == 0) {
            e.selected = true;
        }
    });
}

//
// if some packages get changed, show warnings
//
function warnIfPackageInfoChanges() {
    var numChangedX86 = 0;
    var numUnchangedX86 = 0;

    var packageTabs = $("#packageTabContainer .tab_body").children();
    packageTabs.each(function (index, tab) {
        var langArch = $(tab).find("input:hidden[name^='PackageLangArch_']").val();
        var packageLocationUrl = $(tab).find("input[name$='.PackageUrl']");
        var startPage = $(tab).find("input[name$='.StartPage']");
        var sha1Hash = $(tab).find("input[name$='.SHA1Hash']");


        // Are the current package details different than they were originally?
        var changed = (packageLocationUrl.val() != packageLocationUrl.attr("data-oldvalue")) ||
                      (startPage.val() != startPage.attr("data-oldvalue")) ||
                      (sha1Hash.val() != sha1Hash.attr("data-oldvalue"));

        // If the there were no package details originally, then we don't consider them as having changed.
        // Really, in this case, the user has simply set them for the first time, not changed them.
        var newlySet = (packageLocationUrl.attr("data-oldvalue") == "") &&
                       (startPage.attr("data-oldvalue") == "") &&
                       (sha1Hash.attr("data-oldvalue") == "");

        // If the current package details have been cleared, then we don't consider them as having changed.
        var notSet = (packageLocationUrl.val() == "") &&
                     (startPage.val() == "") &&
                     (sha1Hash.val() == "");

        $("#packageChanged_" + langArch).hide();
        $("#packageUnchanged_" + langArch).hide();
        $("#packageNotSet_" + langArch).hide();
        $("#packageNewlySet_" + langArch).hide();

        if (notSet) {
            $("#packageNotSet_" + langArch).css("display", "inline");
        }
        else if (newlySet) {
            $("#packageNewlySet_" + langArch).css("display", "inline");
        }
        else if (changed) {
            $("#packageChanged_" + langArch).css("display", "inline");
            numChangedX86++;
        }
        else {
            $("#packageUnchanged_" + langArch).css("display", "inline");
            numUnchangedX86++;
        }
    });

    ((numChangedX86 > 0) && (numUnchangedX86 > 0))
        ? $("#packageChangeImbalanceWarningPanel").show()
        : $("#packageChangeImbalanceWarningPanel").hide();
}

//
// Logo and screenshots
//
function replaceLogo() {
    $("#panelSubmittedLogo").hide();
    $("#panelEmptyLogo").show();
    $("#SetLogo").val("True");
}

function replaceScreenshot(index) {
    index = index.toString();

    $("#panelSubmittedScreenshot" + index).hide();
    $("#panelEmtpyScreenshot" + index).show();
    $("#SetScreenshot" + index).val("True");
}

function resetFileUploader(sender) {
    var thePanel = $(sender).parent();

    // file uploader is the first child
    var fu = thePanel.children(":first");

    // if user select a file
    if (fu.get(0).files.length > 0) {
        fu.wrap("<div></div>");
        var fuParent = fu.parent();
        var fuHtml = fuParent.html();
        fuParent.remove();
        $(fuHtml).insertBefore(thePanel.children(":first"));
        thePanel.children(":first").change(function () { validate(); });
    }
}

function addDatepicker(element) {
    $(element).datepicker({
        todayBtn: "linked",
        language: $("#datepickerLanguage").val().toLowerCase() == "zh-chs" ? "zh-CN" : ($("#datepickerLanguage").val().toLowerCase() == "zh-cht" ? "zh-TW" : $("#datepickerLanguage").val())
    });
}