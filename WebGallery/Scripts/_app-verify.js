$(document).ready(function () {
    alert("starting validation...");
    verifyUrls();
    verifyPackages();
    verifyImages($("#ulImages li:lt(2)"), true);
    verifyImages($("#ulImages li:gt(1)"), false);
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

        var ajaxOption = {
            url: "/app/verifyimage",
            type: "post",
            dataType: 'json',
            data: addAntiForgeryToken({
                url: theUrl,
                isLogo: isLogo
            })
        };

        $.ajax(ajaxOption).done(function (verification) {
            if (verification.TypeStatus == "Pass") {
                theUrlLis.each(function (i, li) {
                    $(li).removeClass("validation-validating").addClass("validation-pass"); // the li for image type
                });
            }
            else if (verification.TypeStatus == "Fail") {
                showGoToFix();
                theUrlLis.each(function (i, li) {
                    $(li).removeClass("validation-validating").addClass("validation-fail"); // the li for image type
                });
            }
            else if (verification.TypeStatus == "Unknown") {
                theUrlLis.each(function (i, li) {
                    $(li).removeClass("validation-validating").addClass("validation-unknown"); // the li for image type
                });
            }

            if (verification.DimensionStatus == "Pass") {
                theUrlLis.each(function (i, li) {
                    $(li).next().removeClass("validation-validating").addClass("validation-pass"); // the li for image dimension
                });
            }
            else if (verification.DimensionStatus == "Fail") {
                showGoToFix();
                theUrlLis.each(function (i, li) {
                    $(li).next().removeClass("validation-validating").addClass("validation-fail"); // the li for image dimension
                });
            }
            else if (verification.DimensionStatus == "Unknown") {
                theUrlLis.each(function (i, li) {
                    $(li).next().removeClass("validation-validating").addClass("validation-unknown"); // the li for image dimension
                });
            }
        });
    });
}

verifyPackages = function () {
    var packageUrlArr = [];
    var packageHashArr = [];

    $("#ulPackages li").each(function (index, li) {
        if (index % 2 == 0) {
            var theUrl = $(li).attr("data-value");
            var theHash = $(li).next().attr("data-value");
            if ($.inArray(theUrl, packageUrlArr) > -1
                && $.inArray(theHash, packageHashArr) > -1) return;

            packageUrlArr.push(theUrl);
            packageHashArr.push(theHash);
        }
    });

    $(packageUrlArr).each(function (index, theUrl) {
        var theHash = packageHashArr[index];

        var theUrlLis = $([]);
        $('#ulPackages li[data-value="' + theUrl + '"]').each(function (i, li) {
            if ($(li).next().attr("data-value") == theHash) {
                theUrlLis.push(li);
            }
        });

        theUrlLis.each(function (i, li) {
            $(li).addClass("validation-validating"); // the li for package manifest
            $(li).next().addClass("validation-validating"); // the li for hash
        });

        var ajaxOption = {
            url: "/app/verifypackage",
            type: "post",
            dataType: 'json',
            data: addAntiForgeryToken({
                url: theUrl,
                hash: theHash,
                submissionId: $("#hiddenSubmissionId").val()
            })
        };

        $.ajax(ajaxOption).done(function (verification) {
            if (verification.ManifestStatus == "Pass") {
                theUrlLis.each(function (i, li) {
                    $(li).removeClass("validation-validating").addClass("validation-pass"); // the li for package manifest
                });
            }
            else if (verification.ManifestStatus == "Fail") {
                showGoToFix();
                theUrlLis.each(function (i, li) {
                    $(li).removeClass("validation-validating").addClass("validation-fail"); // the li for package manifest
                });
            }
            else if (verification.ManifestStatus == "Unknown") {
                theUrlLis.each(function (i, li) {
                    $(li).removeClass("validation-validating").addClass("validation-unknown"); // the li for package manifest
                });
            }

            if (verification.HashStatus == "Pass") {
                theUrlLis.each(function (i, li) {
                    $(li).next().removeClass("validation-validating").addClass("validation-pass"); // the li for hash
                });
            }
            else if (verification.HashStatus == "Fail") {
                showGoToFix();
                theUrlLis.each(function (i, li) {
                    $(li).next().removeClass("validation-validating").addClass("validation-fail"); // the li for hash
                });
            }
            else if (verification.HashStatus == "Unknown") {
                theUrlLis.each(function (i, li) {
                    $(li).next().removeClass("validation-validating").addClass("validation-unknown"); // the li for hash
                });
            }
        });
    });
}

verifyUrls = function () {
    var urlArr = [];

    $("#ulUrls li").each(function (index, li) {
        var theUrl = $(li).attr("data-value");
        if ($.inArray(theUrl, urlArr) > -1) return;
        urlArr.push(theUrl);
    });

    $(urlArr).each(function (index, url) {
        var validatingLis = $('#ulUrls li[data-value="' + url + '"]');
        validatingLis.addClass("validation-validating");

        var ajaxOption = {
            url: "/app/verifyurl",
            type: "post",
            dataType: 'json',
            data: addAntiForgeryToken({
                url: url
            })
        };

        $.ajax(ajaxOption).done(function (verification) {
            if (verification.status == "Pass") {
                validatingLis.removeClass("validation-validating").addClass("validation-pass");
            }
            else if (verification.status == "Fail") {
                showGoToFix();
                validatingLis.removeClass("validation-validating").addClass("validation-fail");
            }
            else {
                validatingLis.removeClass("validation-validating").addClass("validation-unknown");
            }
        });
    });
}

addAntiForgeryToken = function (data) {
    data.__RequestVerificationToken = $('input[name=__RequestVerificationToken]').val();
    return data;
}

showGoToFix = function () {
    $("#panelFail").show();
    $("#panelPass").hide();
}

showPublishPanel = function () {
    var isValidatingDone = true;
    var hasValidationFails = false;

    $(".validation-panel li").each(function (index, li) {
        var hasValidationFails = $(li).hasClass("validation-fail");
        if (hasValidationFails)
        {
            return false;
        }

        if (li.className != "validation-pass" && li.className != "validation-fail" && li.className == "validation-unknown")
        {
            isValidatingDone = false;
            return false;
        }
    });

    if (hasValidationFails)
    {
        $("#panelFail").show();
        $("#panelPass").hide();
        return;
    }

    if (isValidatingDone)
    {
        $("#panelFail").hide();
        $("#panelPass").show();
    }        
}