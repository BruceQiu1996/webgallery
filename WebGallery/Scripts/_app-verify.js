var itemsToValidate = $([]);

updateValidationItems = function (key) {
    itemsToValidate.each(function (index, item) {
        if (item.key == key)
            item.status = "done";
    });
}

$(document).ready(function () {
    verifyUrls();
    verifyImages($("#ulImages li:lt(2)"), true);
    verifyImages($("#ulImages li:gt(1)"), false);
    verifyPackages();
});

verifyImages = function (imageLis, isLogo) {
    var imageUrlArr = [];

    imageLis.each(function (index, li) {
        var theUrl = $(li).attr("data-value");
        if ($.inArray(theUrl, imageUrlArr) == -1) {
            imageUrlArr.push(theUrl);
        }
    });

    $(imageUrlArr).each(function (index, theUrl) {
        var theUrlLis = $([]);
        imageLis.each(function (i, li) {
            if (i % 2 == 0 && $(li).attr("data-value") == theUrl) {
                theUrlLis.push(li);
            }
        });

        theUrlLis.each(function (i, li) {
            $(li).addClass("validation-validating"); // the li for image type
            $(li).next().addClass("validation-validating"); // the li for image dimension
        });

        // recored the validating item and its status
        var validatingItemKey = "image@" + theUrl;
        itemsToValidate.push({ key: validatingItemKey, status: "validating" });

        var ajaxOption = {
            url: "/app/images/verify",
            type: "post",
            dataType: 'json',
            data: addAntiForgeryToken({
                url: theUrl,
                isLogo: isLogo,
                key: validatingItemKey,
            })
        };

        $.ajax(ajaxOption).done(function (verification) {
            updateValidationItems(verification.Key);

            theUrlLis.each(function (i, li) {
                $(li).removeClass("validation-validating").addClass("validation-" + verification.TypeStatus.toLowerCase()); // the li for image type
                $(li).next().removeClass("validation-validating").addClass("validation-" + verification.DimensionStatus.toLowerCase()); // the li for image dimension
            });

            showPassPanel();
        });
    });
}

verifyPackages = function () {
    var packageUrlArr = [];
    var packageHashArr = [];

    $("#ulPackages li").each(function (index, li) {
        var theUrl = $(li).attr("data-name");
        var theHash = $(li).attr("data-value");
        if ($.inArray(theUrl, packageUrlArr) > -1
            && $.inArray(theHash, packageHashArr) > -1) return;

        packageUrlArr.push(theUrl);
        packageHashArr.push(theHash);
    });

    $(packageUrlArr).each(function (index, theUrl) {
        var theHash = packageHashArr[index];

        var theUrlLis = $([]);
        $('#ulPackages li[data-name="' + theUrl + '"]').each(function (i, li) {
            if ($(li).attr("data-value") == theHash) {
                theUrlLis.push(li);
            }
        });

        theUrlLis.each(function (i, li) {
            $(li).addClass("validation-validating");
        });

        // recored the validating item and its status
        var validatingItemKey = "package@" + theUrl + "@" + theHash;
        itemsToValidate.push({ key: validatingItemKey, status: "validating" });

        var ajaxOption = {
            url: "/app/packages/verify",
            type: "post",
            dataType: 'json',
            data: addAntiForgeryToken({
                url: theUrl,
                hash: theHash,
                submissionId: $("#hiddenSubmissionId").val(),
                key: validatingItemKey,
            })
        };

        $.ajax(ajaxOption).done(function (verification) {
            updateValidationItems(verification.Key);

            // generate a report with verification.PackageValidation.ValidationEvents
            var html = "<li>";
            html += "<table>";
            html += "<thead><tr><th>Result</th><th>File Name</th><th>Line #</th><th>Message</th></tr></thead>";
            html += "<tbody>";
            for (var i = 0; i < verification.PackageValidation.ValidationEvents.length; i++) {
                var event = verification.PackageValidation.ValidationEvents[i];
                var resultStr = getResultString(event.Type);
                var targetStr = event.Target == null ? "" : event.Target;
                var locationStr = event.Location == null ? "" : event.Location;
                html += "<tr><td class='" + resultStr.toLowerCase() + "'>" + resultStr + "</td><td>" + targetStr + "</td><td>" + locationStr + "</td><td>" + event.Message + "</td></tr>";
            }
            html += "</tbody>";
            html += "</table>";
            html += "</li>";

            var className = "validation-" + verification.Result.toLowerCase();
            theUrlLis.each(function (i, li) {
                $(li).removeClass("validation-validating").addClass(className);
                $(html).insertAfter(li);
            });

            showPassPanel();
        });
    });
}

getResultString = function (type) {
    switch (type) {
        case 0:
            return "Pass";
        case 1:
            return "Fail";
        case 2:
            return "Info";
        case 3:
            return "Exception";
        default:
            return "";
    }
}

verifyUrls = function () {
    var urlArr = [];

    $("#ulUrls li").each(function (index, li) {
        var theUrl = $(li).attr("data-value");
        if ($.inArray(theUrl, urlArr) > -1) return;
        urlArr.push(theUrl);
    });

    $(urlArr).each(function (index, theUrl) {
        var validatingLis = $('#ulUrls li[data-value="' + theUrl + '"]');
        validatingLis.addClass("validation-validating");

        // recored the validating item and its status
        var validatingItemKey = "url@" + theUrl;
        itemsToValidate.push({ key: validatingItemKey, status: "validating" });

        var ajaxOption = {
            url: "/app/urls/verify",
            type: "post",
            dataType: 'json',
            data: addAntiForgeryToken({
                url: theUrl,
                key: validatingItemKey
            })
        };

        $.ajax(ajaxOption).done(function (verification) {
            updateValidationItems(verification.Key);

            validatingLis.removeClass("validation-validating").addClass("validation-" + verification.status.toLowerCase());

            showPassPanel();
        });
    });
}

addAntiForgeryToken = function (data) {
    data.__RequestVerificationToken = $('input[name=__RequestVerificationToken]').val();
    return data;
}

showPassPanel = function () {
    var allValidationsDone = true;
    itemsToValidate.each(function (index, item) {
        allValidationsDone = allValidationsDone && item.status == "done";
    });

    if (allValidationsDone) {
        var hasValidationFails = false;

        $(".validation-panel li").each(function (index, li) {
            hasValidationFails = $(li).hasClass("validation-fail");
            if (hasValidationFails) {
                return false;
            }
        });

        if (hasValidationFails) {
            $("#panelFail").show();
            $("#panelPass").hide();
        }
        else {
            $("#panelFail").hide();
            $("#panelPass").show();
        }
    }
}