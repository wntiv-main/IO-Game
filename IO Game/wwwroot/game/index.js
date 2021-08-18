(function () {
	alert("GAME JOINED");
	var canvas = document.createElement('canvas');
	canvas.width = canvas.style.width = window.innerWidth;
	canvas.height = canvas.style.height = window.innerHeight;
	document.body.appendChild(canvas);
	var c = canvas.getContext('2d');
	//Change aspect ratios so it can fit multiple devices.
	window.addEventListener("resize", function () {
		canvas.width = window.innerWidth;
		canvas.height = window.innerHeight;
	});

	//Socket
	var gameSocket = new WebSocket(location.href.replace("http", "ws"));
	window.gameSocket = gameSocket;
	gameSocket.addEventListener("open", function () {
		console.log("open");
		gameSocket.addEventListener("message", function () {

		});
	});
	gameSocket.addEventListener("close", function (e) {
		console.log("close");
		console.log(e);
		switch (e.reason) {
        }
	});
})();