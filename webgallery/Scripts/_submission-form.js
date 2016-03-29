$(function () {
    addHandlersForTab($("#metaDataTabContainer"));
    addHandlersForTab($("#outerPackageTabContainer"));
    $("#appSubmitContainer input:text,textarea,select,input:file").bind("mouseenter mouseleave", function () { $(this).toggleClass("input-mouseover"); });
    $("#appSubmitContainer input:text,textarea,select").focusin(function () { setExplanationPosition(this, $(this).next(".explanation")); });
    $("#appSubmitContainer input:text,textarea,select").focusout(function () { $(this).next(".explanation").css({ visibility: "hidden" }); });
    $("#appSubmitContainer input:file").focusin(function () { setExplanationPosition(this, $(this).parent().nextAll(".explanation:first")); });
    $("#appSubmitContainer input:file").focusout(function () { $(this).parent().nextAll(".explanation:first").css({ visibility: "hidden" }); });

    //$("#appSubmitContainer input:text,textarea,select").each(function (i, sender) {
    //    $(sender).focusin(function () {
    //        setExplanationPosition(this, $(this).next(".explanation"));
    //    });
    //    $(sender).focusout(function () {
    //        $(this).next(".explanation").css({ visibility: "hidden" });
    //    });
    //    $(sender).bind("mouseenter mouseleave", function () { $(sender).toggleClass("input-mouseover"); });
    //});

    //$("#appSubmitContainer input:file").each(function (i, sender) {
    //    $(sender).focusin(function () {
    //        
    //    });
    //    $(sender).focusout(function () {
    //        
    //    });
    //    $(sender).bind("mouseenter mouseleave", function () { $(sender).toggleClass("input-mouseover"); });
    //});
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

    var remaining = maximum - $(textContainer).val().length;
    if (remaining > 10) {
        countdownElement.className = "low-count";
    }
    else if (remaining > -1) {
        countdownElement.className = "high-count";
    }
    else {
        countdownElement.className = "exceeded-count";
    }

    countdownElement.html(remaining);

    // update form validator
}

function createNickname(nicknameContainer)
{
    alert('Not implemented. Will code this later.');
}