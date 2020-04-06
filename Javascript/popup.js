//Based on this tutorial -> http://yensdesign.com/2008/09/how-to-create-a-stunning-and-smooth-popup-using-jquery/
var popupOpen = false;

$(document).ready(initPopup);

function initPopup() {
    //Add window resize handler
    $(window).resize(onWindowResize);

    //Assign events
    $("#popupBackground").click(disablePopup);
    $("#instructions").click(openInstructions);
    $("#popupContentClose").click(disablePopup);
}

function loadPopup( ) {
    if (popupOpen == false) {
        $("#popupBackground").css(
            { "opacity": "0.7" }
        );

        popupOpen = true;
        centerPopup();
        $("#popupBackground").fadeIn("slow");
        $("#popupContainer").fadeIn("slow");

        window.scrollTo(0, 0); 
    }
}

function disablePopup() {
    if (popupOpen) {
        $("#popupBackground").fadeOut("slow");
        $("#popupContainer").fadeOut("slow");

        popupOpen = false;
    }
}

function centerPopup() {
    //request data for centering  
    var windowWidth = $("#popupContainer").parent().width(); //document.documentElement.clientWidth;
    var windowHeight = $("#popupContainer").parent().height(); //document.documentElement.clientHeight;
    var popupHeight = $("#popupContainer").height();
    var popupWidth = $("#popupContainer").width();

    var top = 50;
    var left = windowWidth / 2 - popupWidth / 2 + 15;

    //centering  
    $("#popupContainer").css({
        "position": "absolute",
        "top": top,
        "left": left
    });
    //only need force for IE6  

    $("#popupBackground").css({
        "height": document.documentElement.clientHeight
    });
}

function onWindowResize() {
    centerPopup();
}

function openInstructions() {
    loadPopup();
    return false;
}