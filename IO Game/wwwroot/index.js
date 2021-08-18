document.getElementById("findGame").addEventListener("click", function () {
    let request = new XMLHttpRequest();
    request.addEventListener("readystatechange", function () {
        if (request.readyState == 4) {
            console.log(request);
        }
    });
    request.open("GET", "/findGame/?" + document.getElementById("gamemode").value);
    request.send();
});