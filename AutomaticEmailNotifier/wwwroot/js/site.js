// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function()
{
    /// Check if incorrect email recipents
    $("#To").focusout(function () { FocusoutEventHandler() });

    /// 
    $("#BtnSubmit").click(function () { ButtonSubmitClickEventHandler() });

});


///---
function ButtonSubmitClickEventHandler() {

    const textSubject = document.getElementById("Subject").value;
    const textBody = document.getElementById("Body").value;

    if (!((!!textSubject) && (!!textBody))) {

        if (confirm("Send this message without a subject or text in the body?")) {
            txt = "You pressed OK!";
        } else {
            txt = "You pressed Cancel!";

            // Prevent form submittion
            event.preventDefault();
        }
        document.getElementById("demo").innerHTML = txt;

    } else {

    }
}


///--- Check if incorrect email recipents
function FocusoutEventHandler() {

    const textTo = document.getElementById("To").value;
    var resultTo = checkAndValidateEmail(textTo);

    if (resultTo.length > 0) {
        document.getElementById("ToAlertMessage").innerHTML = "Invalid emails: " + resultTo;
        return;
    }
}

function checkAndValidateEmail(emailString)
{
    const emailList = emailString.split(";");
    const errorList = [];

    for (let i = 0; i < emailList.length; i++) {
        let email = emailList[i];
        email = email.replace(/^\s+|\s+$/gm, '');

        if (email == "" || validateEmail(email)) {
            continue;
        } else {
            errorList.push(email);
        }
    }

    let result = "";
    for (let i = 0; i < errorList.length; i++) {
        result += "\"" + errorList[i] + "\"";

        if (i < errorList.length - 1) {
            result += ", ";
        }
    }

    return result;
}

const validateEmail = (email) => {
    var re = /\S+@\S+\.\S+/;
    return re.test(email);
};



///--- Unused -----
function KeyupEvent(inputElementId) {
    const inputString = document.getElementById(inputElementId).value;
    var printLabel = "";

    if (inputString.length > 0) {
        printLabel = document.getElementById(inputElementId).getAttribute("name");
    }

    document.getElementById(inputElementId + "Header").innerHTML = printLabel;
}



