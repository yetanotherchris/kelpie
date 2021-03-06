﻿var Kelpie = Kelpie || {};

Kelpie.ErrorMessageExpander = function($)
{
	function init()
	{
		$("table.errors .expand").click(function () {
			var tr = $(this).parent().parent();
			var nextRow = tr.next();
			var id = tr.attr("id");
			var pre = nextRow.find(".message");

			if (pre.text() !== "") {
				tr.toggleClass("selected");
				nextRow.toggle();
				return;
			}
			else {
				$.ajax({
					method: "GET",
					url: "/App/LoadMessage",
					data: { "id": id }
				})
				.done(function (msg) {
					tr.next().find(".message").html(msg);
				});

				tr.addClass("selected");
				nextRow.removeClass("hide");
				nextRow.show();
			}
		});
	}

	return {
		init: init
	}
}($);