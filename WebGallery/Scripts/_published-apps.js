$(document).ready(function () {
    $(".search-form form p input").change(function () {
        $(this).parent().parent().submit();
    });
    $(".search-form form p select").change(function () {
        $(this).parent().parent().submit();
    });
    $(".apps-table-container table tbody tr td:last-child").children("a").click(function () {
        var appId = $(this).parent().parent().children("td:first");
        var logoUrl = $(this).parent().children("input").val();
        $(".app-remove-detail span").text(appId.text() + " " + appId.next().next().text());
        $(".app-remove-detail p strong").text(" " + appId.next().next().next().children("a").text());
        $(".icon-box img").attr("src", logoUrl);
        $(".confirm-content form input:nth-child(2)").val(appId.text());
        $(".confirm-content span").click(function () {
            $(".confirm-dialog").attr("style", "display:none");
        });
        $(".confirm-content input").last().click(function () {
            $(".confirm-dialog").attr("style", "display:none");
        });
        $(".confirm-dialog").removeAttr("style");
    });
})