$(".changeAccommodationType").on("click", function () {
    // Pobieramy wartość atrybutu data-id
    var accommodationTypeID = $(this).data("id");

    // Ukrywamy wszystkie kontenery (div) dla typów
    $(".accommodationTypesRow").hide();

    // Pokazujemy tylko ten kontener, który ma data-id równy pobranemu typowi
    $("div.accommodationTypesRow[data-id='" + accommodationTypeID + "']").show();
});
