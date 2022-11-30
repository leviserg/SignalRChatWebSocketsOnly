document.querySelector('#name').value = "";

let userName = prompt("Please enter your name:", "Harry Potter");

$(document).ready(function () {
    $("#name").val(userName);
});

let socket = null;

/*
socket.addEventListener('open', function (event) {
    console.log('Connected to server');
});
*/
function connect() {

    if (socket != null && socket.readyState === 1) {
        try {
            socket.close();
            socket.addEventListener('close', function (event) {
                console.log('Disconnected from server');
                document.querySelector('#conState').textContent = 'Disconnected';
                document.querySelector('#conState').style.color = 'red';
                document.querySelector('#connectButton').textContent = 'Connect';
            });
            socket = null;
        }
        catch (error) {
            console.log(error);
        }
    } else {

        socket = new WebSocket('ws://localhost:5065/messages?username=' + userName);

        try {

            socket.addEventListener('open', function (event) {
                console.log('Connected to server');
                document.querySelector('#conState').textContent = 'Connected';
                document.querySelector('#conState').style.color = 'green';
                document.querySelector('#connectButton').textContent = 'Disconnect';
            });

            socket.addEventListener('message', function (event) {
                console.log('Message from server ', event.data);
                let message = JSON.parse(event.data);
                if (message.Command === 'Send')
                    appendMessageToTextArea(message.CreatedAt, message.Sender, message.Body, 'black');
            });

        }
        catch (error) {
            console.log(error);
        }
    }
};

async function sendMessage() {

    if (socket.readyState === 1) {
        let inputText = document.querySelector('#message');
        let message = inputText.value;
        let chatMessage = {
            Command: 'SendToOthers',
            Body: message,
            Sender: $("#name").val()
        };
        try {
            await socket.send(JSON.stringify(chatMessage));
            let d = new Date();
            appendMessageToTextArea(time_format(d),'Me', message, 'green');
        }
        catch (error) {
            console.log(error);
        }
        document.querySelector('#message').value = '';
    }

}

function appendMessageToTextArea(messageDate, sender, message, color) {
    //document.querySelector('#messages-content').insertAdjacentHTML("beforeend", `<div style="color:${color}" class="py-0,my-0">${sender} : ${message}<br></div><br>`);
    document.querySelector('#messages-content').value += messageDate + ' : ' + sender + ' : ' + message + '\n';
    scrollToBottom(document.querySelector('#messages-content'));
}

function scrollToBottom(elem) {
    elem.scrollTop = elem.scrollHeight;
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

function time_format(d) {
    hours = format_two_digits(d.getHours());
    minutes = format_two_digits(d.getMinutes());
    seconds = format_two_digits(d.getSeconds());
    return hours + ":" + minutes + ":" + seconds;
}

function format_two_digits(n) {
    return n < 10 ? '0' + n : n;
}

function response_time_format(d) {
    return d.substr(d.indexOf(":") - 2, 8); //.inhours + ":" + minutes + ":" + seconds;
}
