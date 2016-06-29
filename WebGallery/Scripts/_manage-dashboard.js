$(document).ready(function () {
    $(".submissions-table tbody tr td:last-child").mouseenter(function () { showActions($(this).find("a").get(0), $(this).find("div")); });
    $(".submissions-table tbody tr td:last-child").mouseleave(function () { $(this).find("div").hide(); });
    $(".submissions-table tbody tr td:nth-last-child(2)").find("a:first").click(function () {
        $(this).parent().prev().find('select').show();
        $(this).parent().prev().find('span').hide();
        $(this).hide();
        $(this).next().removeClass('hide');
        $(this).next().next().removeClass('hide');
        $(this).next().show();
        $(this).next().next().show();
    });
    $(".submissions-table tbody tr td:nth-last-child(2)").find("a:first").next().click(function () {
        if ($(this).parent().prev().find('select').find('option:selected').text() == $(this).parent().prev().find('span').text()) {
            $(this).parent().prev().find('select').hide();
            $(this).parent().prev().find('span').show();
            $(this).prev().show();
            $(this).hide();
            $(this).next().hide();
        }
        else {
            $(this).parent().prev().find('form').find('input:first-child').val($(this).parent().prev().find('select').find('option:selected').text());
            $(this).parent().prev().find('form').submit();
        }
    });
    $(".submissions-table tbody tr td:nth-last-child(2)").find("a:last").click(function () {
        $(this).prev().prev().show();
        $(this).hide();
        $(this).prev().hide();
        $(this).parent().prev().find('select').find('option:selected').removeAttr("selected");
        $(this).parent().prev().find('select').find('option').each(function () {
            if ($(this).text() == $(this).parent().parent().prev().text()) {
                $(this).attr("selected", "selected");
            }
        });
        $(this).parent().prev().find('span').show();
        $(this).parent().prev().find('select').hide();
    });
    $(".submissions-table tbody tr td:last-child").find("ul").find("li:last").find("a").click(function () {
        $(this).parent().parent().find("form").submit();
    });
    $(".search-form form p input").change(function () {
        $(this).parent().parent().submit();
    });
    $(".search-form form p select").change(function () {
        $(this).parent().parent().submit();
    });
});

function showActions(target, panelActions) {
    var left = target.offsetWidth
        + target.offsetLeft
        + target.offsetParent.offsetLeft
        + target.offsetParent.offsetParent.offsetLeft;
    var top = target.offsetTop
        + target.offsetParent.offsetTop
        + target.offsetParent.offsetParent.offsetTop;
    panelActions.css({ left: left + "px", top: top + "px" });
    panelActions.show();
}