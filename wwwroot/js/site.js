function loadPage(pageNumber) {
    $.ajax({
        url: inventoryListUrl,
        data: { pageNumber: pageNumber },
        success: function (data) {
            $('#inventoryListContainer').html(data);
        },
        error: function (xhr, status, error) {
            console.error("Error occurred: " + error);
        }
    });
}

$(document).ready(function () {
    // Event handler for clicking the search button
    $('#searchButton').click(function () {
        // Retrieve values from search input fields
        var itemId = $('#searchItemId').val();
        var itemName = $('#searchItemName').val();

        // AJAX request to search inventory items
        $.ajax({
            url: '/Inventory/SearchInventory', // Endpoint for the search operation
            type: 'GET', // Method type GET for fetching data
            data: {
                searchTerm: itemName, // Sending the search term for item name
                itemId: itemId // Sending the item ID for search
            },
            success: function (data) {
                // Update the inventory list with the received HTML content
                $('#inventoryListContainer').html(data);
            },
            error: function (xhr, status, error) {
                // Log the error for debugging purposes
                console.error("An error occurred: " + error);
            }
        });
    });

    // Handling double-click event on dynamically loaded inventory rows
    $('#inventoryListContainer').on('dblclick', '.item-row', function () {
        // Extract item ID from the data attribute of the clicked row
        var itemId = $(this).data('item-id');
        // Redirect to the item edit page using the item ID
        window.location.href = '/Inventory/EditItem/' + itemId;
    });

    // Handling single-click event on inventory rows for selection
    $('#inventoryListContainer').on('click', '.item-row', function () {
        // Clear highlighting from all rows to ensure only one is selected at a time
        $('.item-row').removeClass('highlighted-row');

        // Add highlighting to the clicked (selected) row
        $(this).addClass('highlighted-row');
    });
});