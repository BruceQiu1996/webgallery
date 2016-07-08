$(document).ready(function () {
    $(".submissions-table tbody tr").bind("mouseenter mouseleave", function () { $(this).toggleClass("row-mouseover"); });
    $(".submissions-table tbody tr td:last-child").mouseenter(function () { showActions($(this).find("a").get(0), $(this).find("div")); });
    $(".submissions-table tbody tr td:last-child").mouseleave(function () { $(this).find("div").hide(); });
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