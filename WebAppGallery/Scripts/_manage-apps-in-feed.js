﻿$(document).ready(function () {
    $(".search-form form p input").change(function () {
        this.form.submit();
    });
    $(".search-form form p select").change(function () {
        this.form.submit();
    });

    $(".apps-table-container input[value='Rebrand']").click(function () {
        var appId = $(this).parent().siblings("td:first");
        var logoUrl = $(this).siblings("input:last").val();

        $(".rebrand-confirm-content form").attr("action", "/admin/apps/in/feed/" + appId.text() + "/rebrand");
        $(".rebrand-app-detail span").text(appId.text() + " " + appId.next().next().text());
        $(".rebrand-app-detail p strong").text(" " + appId.next().next().next().children("a").text());
        $(".rebrand-app-detail .icon-box img").attr("src", logoUrl);
        $(".rebrand-container input:first").val(appId.text());
        $(".rebrand-container input:last").val("");

        $(".rebrand-confirm-content span:first").click(function () {
            $(".rebrand-confirm-dialog").attr("style", "display:none");
        });
        $(".rebrand-confirm-content input[type=button]").click(function () {
            $(".rebrand-confirm-dialog").attr("style", "display:none");
        });
        $(".rebrand-confirm-dialog").attr("style", "display:display");
    });

    $(".apps-table-container input[value='Delete']").click(function () {
        var appId = $(this).parent().siblings("td:first");
        var logoUrl = $(this).siblings("input:last").val();

        $(".confirm-content form").attr("action", "/admin/apps/in/feed/" + appId.text() + "/delete");
        $(".app-remove-detail span").text(appId.text() + " " + appId.next().next().text());
        $(".app-remove-detail p strong").text(" " + appId.next().next().next().children("a").text());
        $(".app-remove-detail .icon-box img").attr("src", logoUrl);
        $(".related-submissions-container").empty();
        $(".related-submissions-container").addClass("submissions-loading");

        $.ajax({
            type: "GET",
            url: "/admin/apps/" + appId.text() + "/submissions",
            cache: false,
            success: function (viewHTML) {
                $(".related-submissions-container").removeClass("submissions-loading");
                $(".related-submissions-container").html(viewHTML);
                colWidth = $(".related-submissions-container table tbody tr:first").children().map(function () {
                    return $(this).width();
                }).get();
                $(".related-submissions-container table thead tr").children().each(function (i, v) {
                    $(v).width(colWidth[i]);
                });
            },
            error: function () {
                $(".related-submissions-container").removeClass("submissions-loading");
                $(".related-submissions-container").html("<p>Loading submissions failed, please try open this dialog again.</p>");
            }
        });

        $(".confirm-content span:first").click(function () {
            $(".confirm-dialog").attr("style", "display:none");
        });
        $(".confirm-content input[type=button]").click(function () {
            $(".confirm-dialog").attr("style", "display:none");
        });
        $(".confirm-dialog").attr("style", "display:display");
    });
})