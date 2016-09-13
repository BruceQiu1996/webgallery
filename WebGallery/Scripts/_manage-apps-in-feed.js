$(document).ready(function () {
    $(".search-form form p input").change(function () {
        this.form.submit();
    });
    $(".search-form form p select").change(function () {
        this.form.submit();
    });
    $(".apps-table-container table tbody tr td:last-child").children("input:first-child").click(function () {
        var appId = $(this).parent().siblings("td:first");
        var logoUrl = $(this).siblings("input:last").val();
        var form = $(this).next();
        $(".app-remove-detail span").text(appId.text() + " " + appId.next().next().text());
        $(".app-remove-detail p strong").text(" " + appId.next().next().next().children("a").text());
        $(".icon-box img").attr("src", logoUrl);
        $(".confirm-content span:first").click(function () {
            $(".confirm-dialog").attr("style", "display:none");
        });
        $(".confirm-content input:last").click(function () {
            $(".confirm-dialog").attr("style", "display:none");
        });
        $(".confirm-content input:first").click(function () {
            form.submit();
        })
        $(".confirm-dialog").attr("style", "display:display");
    });
})