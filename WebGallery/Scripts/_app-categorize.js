$(document).ready(function () {

    if ($("#currentpage").val() == "1") {
        $("#currentpage").next().next().next().removeAttr("href");
        $("#currentpage").next().next().next().addClass("link-disabled");
    }
    if ($("#currentpage").val() == $("#totalpage").val() || $("#totalpage").val() == "0") {
        $("#totalpage").next().removeAttr("href");
        $("#totalpage").next().addClass("link-disabled");
    }
    $(".categories").find("a").each(function () {
        if ($(this).text().toLowerCase() == $(this).parent().parent().find("input:first").val().toLowerCase()) {
            $(this).addClass("this-category");
            $(this).removeAttr("href");
        }
    });
    $(".categories div select").change(function () {
        $(this).parent().submit();
    });
    if (localStorage['page'] == document.URL) {
        $(document).scrollTop(localStorage['scrollTop']);
    }
})

$(document).scroll(function () {
    localStorage['page'] = document.URL;
    localStorage['scrollTop'] = $(document).scrollTop();
});