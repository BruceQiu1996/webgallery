$(document).ready(function () {
    $(".search-form form p input").change(function () {
        this.form.submit();
    });
    $(".search-form form p select").change(function () {
        this.form.submit();
    });

    $(".submitters-table-container td:last-child input:first-child").click(function () {
        $(this).hide();
        $(this).next().attr("style", "display:display");
        $(this).next().next().attr("style", "display:display");
        $(this).parent().prev().children("span").hide();
        $(this).parent().prev().children("form").attr("style", "display:display");
    });

    $(".submitters-table-container td:last-child input:first-child").next().click(function () {
        $(this).parent().prev().find("form").submit();
    });

    $(".submitters-table-container td:last-child input:last-child").click(function () {
        $(this).attr("style", "display:none");
        $(this).prev().attr("style", "display:none");
        $(this).prev().prev().show();
        $(this).parent().prev().children("span").show();
        $(this).parent().prev().find("form").attr("style", "display:none");
        $(this).parent().prev().find("form input:last-child").val($(this).parent().prev().children("span").text());
    });
})