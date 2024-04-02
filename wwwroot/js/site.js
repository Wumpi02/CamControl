// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const notifyPropertyChangedEvent = new CustomEvent("notifyPropertyChanged", {
    detail: { property: "", value: "" }
}); 
var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
    return new bootstrap.Popover(popoverTriggerEl)
})
var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl)
})


const connection = new signalR.HubConnectionBuilder()
    .withUrl("/obsHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {
        await connection.start();
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
    connection.on("NotifyPropertyChanged", function (prop, val) {
        notifyPropertyChangedEvent.detail.property = prop;
        notifyPropertyChangedEvent.detail.value = val;
        document.dispatchEvent(notifyPropertyChangedEvent);  
    });
};

connection.onclose(async () => {
    await start();
});

start();