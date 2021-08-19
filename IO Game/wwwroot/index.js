document.getElementById("findGame").addEventListener("click", function () {
    let request = new XMLHttpRequest();
    request.addEventListener("readystatechange", function () {
        if (request.readyState == 4) {
            location.href = ("/game/?" + request.responseText);
        }
    });
    request.open("POST", "/findGame");
    request.send(document.getElementById("gamemode").value);
});