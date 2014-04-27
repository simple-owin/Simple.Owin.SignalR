$(function () {
    $('#form').hide();
    var hub = $.connection.demoHub;
    hub.client.reply = function (response) {
        $('#responses').append($('<li>' + response + '</li>'));
    };
    $.connection.hub.start().done(function () {
        $('#connecting').hide();
        $('#form').show();
        $('#form').submit(function(event) {
            event.preventDefault();
            hub.server.say($('#something').val());
            $('#something').val('');
        });
    });
});