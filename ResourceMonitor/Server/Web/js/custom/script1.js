var currentComputer = sessionStorage.getItem("currentComputer", currentComputer);
var computerList = [];
var firstTime = true;
var json_data = undefined;

var extraCpu = JSON.parse('{"Name": "Intel Pentium G620","SubHardware": {},"Sensors": {"Voltage": {},"Clock": {"MaxClockSpeed": "2600","CPU Core #1": {"Value": 2594.10669},"CPU Core #2": {"Value": 2594.10669},"Bus Speed": {"Value": 99.77333},"Average Clock": 2594.106689453125},"Temperature": {"MaxTemperature": "102","CPU Core #1": {"Value": 38.0},"CPU Core #2": {"Value": 41.0},"CPU Package": {"Value": 42.0}},"Load": {"CPU Core #1": {"Value": 0.0},"CPU Core #2": {"Value": 100.0},"CPU Total": {"Value": 50.0}},"Fan": {},"Flow": {},"Control": {},"Level": {},"Factor": {},"Power": {"MaxTDP": "65","CPU Package": {"Value": 17.08072},"CPU Cores": {"Value": 12.9638481},"CPU Graphics": {"Value": 0.1728383}},"Data": {},"SmallData": {},"Throughput": {}},"Core Number": 2}');

var firstLoad = sessionStorage.getItem("first_time");

function isMobile() {
    try {
        document.createEvent("TouchEvent");
        return true;
    } catch (exception) {
        return false
    }
}

$(window).resize(function () {
    const element = document.getElementById("computer_container");
    const offset = window.innerHeight - element.offsetTop;
    element.setAttribute('style', `height: ${offset}px`);
});

$(document).ready(function () {
    const element = document.getElementById("computer_container");
    const offset = window.innerHeight - element.offsetTop;
    element.setAttribute('style', `height: ${offset}px`);

    if (!firstLoad) {
        Object.values(cardTypes).forEach((value) => {
            DisableCard(value);
        });
    }

    $("#listSearch").on("keyup", function () {
        var value = $(this).val().toLowerCase();
        $("#computer_container li").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
    });

    $("#sidebar_toggle").on("click", function () {
        if (!firstLoad) {
            Swal.fire({
                icon: "info",
                text: 'Selecione um computador antes de continuar!'
            });
            return;
        }
        $("#sidebar").toggleClass("active");
        $('#content').toggleClass('moved');
        if (isMobile()) {
            $('#sidebar_toggle').removeClass("not_mobile");
        } else {
            $('#sidebar_toggle').addClass("not_mobile");
        }
        if ($('#sidebar_toggle').hasClass("fa-arrow-right")) {
            $('#sidebar_toggle').removeClass("fa-arrow-right");
            $('#sidebar_toggle').addClass("fa-arrow-left");
        } else {
            $('#sidebar_toggle').removeClass("fa-arrow-left");
            $('#sidebar_toggle').addClass("fa-arrow-right");
        }
        //$('#sidebar_toggle').toggle();
    });

    $("#darkTheme").on("click", function () {
        $('#content').toggleClass('dark');
        $('.card').toggleClass('dark');
        $('.dropdown-toggle').toggleClass('dark');
        $('.progress').toggleClass('dark');
        $('#sidebar').toggleClass('dark');
        $('#sidebar .sidebar-header').toggleClass('dark');
        toggleChartDark();
    });

    if (isMobile()) {
        $(".swipe-area").swipe({
            allowPageScroll: "vertical",
            swipeRight: function (event, direction, distance, duration, fingerCount, finderData, currentDirection) {
                $("#sidebar").removeClass("active");
                $('#content').addClass('moved');
                if ($(".navbar-collapse").hasClass("navbar-collapse collapse show")) {
                    $(".navbar-toggler").click();
                }

                $('#sidebar_toggle').removeClass("fa-arrow-right");
                $('#sidebar_toggle').addClass("fa-arrow-left");
            },
            swipeLeft: function (event, direction, distance, duration, fingerCount, finderData, currentDirection) {
                if (!firstLoad) {
                    Swal.fire({
                        icon: "info",
                        text: 'Selecione um computador antes de continuar!'
                    });
                    return;
                }
                $("#sidebar").addClass("active");
                $('#content').removeClass('moved');
                if ($(".navbar-collapse").hasClass("navbar-collapse collapse show")) {
                    $(".navbar-toggler").click();
                }
                $('#sidebar_toggle').removeClass("fa-arrow-left");
                $('#sidebar_toggle').addClass("fa-arrow-right");
            }
        });
    }

    $(document).click(function (event) {
        var clickover = $(event.target);
        var isOpen = $(".navbar-collapse").hasClass("navbar-collapse collapse show");
        if (isOpen === true && !clickover.hasClass("navbar")) {
            $(".navbar-toggler").click();
        }
    });

    Loading();

    setInterval(LoadData, 1000);
});

var trackColor = "#d4d4d4";
var theme = "light"

function toggleChartDark() {
    if (theme == "light") {
        theme = "dark";
    } else {
        theme = "light";
    }
}

$('.chart').easyPieChart({
    size: 135,
    scaleColor: false,
    trackColor: "#d4d4d4",
    lineWidth: 13,
    onStep: function (from, to, percent) {
        if (this.el.classList.contains("disabled")) {
            $(this.el).find('.value').text("-");
        }
        else {
            $(this.el).find('.value').text(Math.round(percent) + "%");
        }
    },
    barColor: function (percent) {
        if (percent >= 100) {
            return "#FF0000";
        } else if (percent <= 0) {
            return "#00FF00";
        }
        return perc2color(percent, 100, 0);
    }
});

function perc2color(perc, min, max) {
    if (perc >= 100) {
        return "#FF0000";
    } else if (perc <= 0) {
        return "#00FF00";
    }

    var base = (max - min);

    if (base == 0) {
        perc = 100;
    } else {
        perc = (perc - min) / base * 100;
    }
    var r, g, b = 0;
    if (perc < 50) {
        r = 255;
        g = Math.round(5.1 * perc);
    } else {
        g = 255;
        r = Math.round(510 - 5.10 * perc);
    }
    var h = r * 0x10000 + g * 0x100 + b * 0x1;
    return '#' + ('000000' + h.toString(16)).slice(-6);
}

(function ($) {
    $.fn.progressBar = function (givenValue) {
        const $this = $(this);

        function init(selector) {
            const progressValue = selector.children().attr('aria-valuenow');

            selector.children().width(progressValue + "%");
        }

        function set(selector, value) {
            var progress_bar = selector.children();
            progress_bar.removeClass('success fail active');
            progress_bar.attr('aria-valuenow', value);

            progress_bar[0].style.backgroundColor = perc2color(value, 100, 0);

            init(selector);
        }

        set($this, givenValue);
    }
}(jQuery));



var computerData = [];
var globalData = [];
var loadOneTime = false;
var sidebarBuilded = false;

var previousData = {
    Computadores: [],
    Quantidade: 0,
};
var currentComputers = {
    Computadores: [],
    Quantidade: 0,
};
function BuildSideBar() {
    if (firstTime) {
        $.ajaxSetup({
            async: false
        });
    }

    $.getJSON('http://samjviana.ddns.net:8084/computerList.json', function (data) {
        currentComputers = data;

        if ((data["Quantidade"] == 0)) {
            return;
        }

        if (previousData["Quantidade"] != data["Quantidade"]) {
            $("#computer_list").html("");
        }

        for (let i = 0; i < data["Quantidade"]; i++) {
            let online = data["Computadores"][i]["Estado"] ? "green-text" : "red-text";
            let pcItem = GetFormattedPcItem(data["Computadores"][i]["Nome"], online);

            if (!($.contains($("#computer_list")[0], $("#" + data["Computadores"][i]["Nome"])[0]))) {
                $("#computer_list").append(pcItem);
            }
            document.getElementById(data["Computadores"][i]["Nome"] + "_online").classList.remove("green-text", "red-text");
            document.getElementById(data["Computadores"][i]["Nome"] + "_online").classList.add(online);

            if (sessionStorage.getItem("currentComputer") != null) {
                if (sessionStorage.getItem("currentComputer") == data["Computadores"][i]["Nome"]) {
                    document.getElementById(data["Computadores"][i]["Nome"]).parentElement.classList.add("active");
                }
            }
        }

        previousData = data;
    });
}

function GetFormattedPcItem(name, online) {
    return `
    <li>
        <a id="${name}" onclick="SetCurrentComputer(this)" class="d-flex flex-row pl-2 pr-2">
            <div class="pl-1 pr-2">
                <i id="${name}_online" class="fas fa-circle ${online}"></i>
            </div>
            ${name}
        </a>
    </li>
    `;
}

var computerChanged = false;

function SetCurrentComputer(element) {
    let sidebar = document.getElementById("computer_list");
    let selectedId = element.getAttribute("id");

    for (let i = 0; i < currentComputers.Quantidade; i++) {
        if (currentComputers.Computadores[i].Nome == selectedId) {
            if (!currentComputers.Computadores[i].Estado) {
                Swal.fire({
                    icon: 'error',
                    title: 'Oops...',
                    text: 'Esse computador está Offline!'
                })
                return;
            }
        }
    }

    let activeComputer = sidebar.getElementsByClassName("active")[0];
    let previousComputer = undefined;

    if (activeComputer != undefined) {
        previousComputer = activeComputer.firstElementChild;
        activeComputer.classList.remove("active");
    }

    element.parentElement.classList.add("active");

    currentComputer = selectedId;
    sessionStorage.setItem("currentComputer", currentComputer);

    if (!firstLoad || (currentComputer != previousComputer.getAttribute("id"))) {
        cpuDropdownBuilded = false;
        computerChanged = true;
        Loading();
    }
}

function Loaded() {
    $('#preloader .inner').fadeOut();
    $('#preloader').delay(0).fadeOut('slow');
    $('body').delay(250).css({
        'overflow': 'visible'
    });

    if (!firstLoad) {
        $("#sidebar").removeClass("active");
        $('#content').addClass('moved');
        if (isMobile()) {
            $('#sidebar_toggle').removeClass("not_mobile");
        } else {
            $('#sidebar_toggle').addClass("not_mobile");
        }
        $('#sidebar_toggle').removeClass("fa-arrow-right");
        $('#sidebar_toggle').addClass("fa-arrow-left");
    }
    if (computerChanged && !firstLoad) {
        computerChanged = false;

        $("#sidebar").addClass("active");
        $('#content').removeClass('moved');
        if (isMobile()) {
            $('#sidebar_toggle').removeClass("not_mobile");
        } else {
            $('#sidebar_toggle').addClass("not_mobile");
        }
        $('#sidebar_toggle').addClass("fa-arrow-right");
        $('#sidebar_toggle').removeClass("fa-arrow-left");

        // first time loaded!
        console.log("first time");
        sessionStorage.setItem("first_time", "1");
        firstLoad = true;

        Object.values(cardTypes).forEach((value) => {
            EnableCard(value);
        });
    }
    if (computerChanged) {
        computerChanged = false;

        $("#sidebar").addClass("active");
        $('#content').removeClass('moved');
        if (isMobile()) {
            $('#sidebar_toggle').removeClass("not_mobile");
        } else {
            $('#sidebar_toggle').addClass("not_mobile");
        }
        $('#sidebar_toggle').addClass("fa-arrow-right");
        $('#sidebar_toggle').removeClass("fa-arrow-left");
    }
}

var doneLoading = false;

function Loading() {
    doneLoading = false;

    $("#preloader .inner").fadeIn();
    $("body").delay(250).css({
        "overflow": "hidden"
    });
    $("#preloader").delay(250).fadeIn(function () {
        doneLoading = true;

        var hdd_content = document.getElementById("hdd_content");
        hdd_content.innerHTML = "";

        setTimeout(LoadData, 3000);

        setTimeout(Loaded, 1500);
    });
}

function LoadData() {
    sidebarBuilded = false;
    BuildSideBar();

    if (currentComputer == undefined) {
        return;
    }
    if (!firstLoad && !computerChanged) {
        return;
    }

    $.getJSON('http://samjviana.ddns.net:8084/' + currentComputer + ".json", function (data) {
        json_data = data;
        if (currentComputer == "PC-SAMUEL") {
            json_data["Hardware"]["CPU"][1] = extraCpu;
        }

        let innerText = currentComputer;
        $('#computer_name').html(innerText);

        /*
         * CPU
         */
        if (IsEnabled(cardTypes.CPU)) {
            BuildCpuCard();
        }
        /*
         * GPU
         */
        if (json_data["Hardware"]["GpuNvidia"].length != 0) {
            var gpu_type = "GpuNvidia";
            EnableCard(cardTypes.GPU);
        } else if (json_data["Hardware"]["GpuAti"].length != 0) {
            var gpu_type = "GpuAti";
            EnableCard(cardTypes.GPU);
        } else {
            DisableCard(cardTypes.GPU);
            var gpu_type = "not found";
        }
        if (gpu_type != "not found" && IsEnabled(cardTypes.GPU)) {
            var gpu_model = json_data["Hardware"][gpu_type][0]["Name"];
            var gpu_load = parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Load"]["GPUCore"]["Value"]).toFixed(1);
            var gpu_memory_load = parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Load"]["GPUMemory"]["Value"]).toFixed(1);
            var gpu_temperature = parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Temperature"]["GPUCore"]["Value"]).toFixed(1);
            var gpu_max_temperature = parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Temperature"]["Maximum"]).toFixed(1);
            var gpu_core_clock = parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Clock"]["GPUCore"]["Value"]).toFixed(1);
            var gpu_max_core_clock = (parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Clock"]["GPUCore"]["Maximum"])).toFixed(1);
            var gpu_memory_clock = parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Clock"]["GPUMemory"]["Value"]).toFixed(1);
            var gpu_max_memory_clock = (parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Clock"]["GPUMemory"]["Maximum"])).toFixed(1);

            document.getElementById("gpu_model").innerHTML = gpu_model;
            document.getElementById("gpu_memory_load_value").innerHTML = gpu_memory_load + " %";
            document.getElementById("gpu_temperature_value").innerHTML = gpu_temperature + " °C";
            document.getElementById("gpu_core_clock_value").innerHTML = gpu_core_clock + " MHz";
            document.getElementById("gpu_memory_clock_value").innerHTML = gpu_memory_clock + " MHz";

            $('#gpu_memory_load').progressBar(gpu_memory_load);
            $('#gpu_temperature').progressBar(parseFloat(mapValue(gpu_temperature, [0, gpu_max_temperature], [0, 100])));
            $('#gpu_core_clock').progressBar(parseFloat(mapValue(gpu_core_clock, [0, gpu_max_core_clock], [0, 100])));
            $('#gpu_memory_clock').progressBar(parseFloat(mapValue(gpu_memory_clock, [0, gpu_max_memory_clock], [0, 100])));

            var gpu_chart = window.chart = $('#gpu_load').data('easyPieChart');
            gpu_chart.update(gpu_load);
        }
        /*
         * RAM
         */
        if (IsEnabled(cardTypes.RAM)) {
            var ram_model = json_data["Hardware"]["RAM"][0]["Name"];
            var ram_load = parseFloat(json_data["Hardware"]["RAM"][0]["Sensors"]["Load"]["Memory"]["Value"]).toFixed(1);
            var free_ram = parseFloat(json_data["Hardware"]["RAM"][0]["Sensors"]["Data"]["Available Memory"]["Value"]).toFixed(1);
            var used_ram = parseFloat(json_data["Hardware"]["RAM"][0]["Sensors"]["Data"]["Used Memory"]["Value"]).toFixed(1);
            var total_ram = parseFloat(json_data["Hardware"]["RAM"][0]["Sensors"]["Data"]["TotalMemory"]).toFixed(1);

            document.getElementById("ram_model").innerHTML = ram_model;
            document.getElementById("total_ram").innerHTML = "Total RAM: " + total_ram + " GB";
            document.getElementById("free_ram").innerHTML = "Free RAM: " + free_ram + " GB";
            document.getElementById("used_ram").innerHTML = "Used RAM: " + used_ram + " GB";

            var ram_chart = window.chart = $('#ram_load').data('easyPieChart');
            ram_chart.update(ram_load);
        }
        /*
         * HDD
         */
        if (json_data["Hardware"]["HDD"].length == 0) {
            DisableCard(cardTypes.HDD);
        }
        else {
            for (let i = 0; i < json_data["Hardware"]["HDD"].length; i++) {
                if (json_data["Hardware"]["HDD"][i]["Letters"] == "-1") {
                    continue;
                }
                if (document.getElementById("hdd" + i) == null) {
                    var hdd = document.createElement("div");
                    hdd.setAttribute("id", "hdd" + i);
                    hdd.setAttribute("class", "my-auto text-center py-1");

                    var rowDiv = document.createElement("div");
                    rowDiv.setAttribute("class", "row d-flex justify-content-between ml-1 mr-1");

                    var model = document.createElement("small");
                    var disk = document.createElement("small");
                    var storage = document.createElement("small");
                    model.setAttribute("id", "hdd" + i + "_model_label");
                    disk.setAttribute("id", "hdd" + i + "_disk_label");
                    storage.setAttribute("id", "hdd" + i + "_storage_label");

                    rowDiv.appendChild(model);
                    rowDiv.appendChild(disk);
                    rowDiv.appendChild(storage);

                    var hdd_progress = document.createElement("div");
                    hdd_progress.setAttribute("id", "hdd" + i + "_progress");
                    hdd_progress.setAttribute("class", "progress md-progress my-1 z-depth-0 border-0");

                    var hdd_bar = document.createElement("div");
                    hdd_bar.setAttribute("id", "hdd" + i + "_bar");
                    hdd_bar.setAttribute("class", "progress-bar z-depth-0");
                    hdd_bar.setAttribute("role", "progressbar");
                    hdd_bar.setAttribute("aria-valuenow", "0");
                    hdd_bar.setAttribute("aria-valuemin", "0");
                    hdd_bar.setAttribute("aria-valuemax", "100");

                    hdd_progress.appendChild(hdd_bar);

                    hdd.appendChild(rowDiv);
                    hdd.appendChild(hdd_progress);

                    document.getElementById("hdd_content").appendChild(hdd);
                }

                try {
                    var model = document.getElementById("hdd" + i + "_model_label");
                    var disk = document.getElementById("hdd" + i + "_disk_label");
                    var storage = document.getElementById("hdd" + i + "_storage_label");
                    model.innerHTML = json_data["Hardware"]["HDD"][i]["Name"];
                    disk.innerHTML = json_data["Hardware"]["HDD"][i]["Letters"];
                    storage.innerHTML = parseFloat(json_data["Hardware"]["HDD"][i]["Sensors"]["Load"]["Used Space"]["Value"]).toFixed(1) + " %";
                    $('#hdd' + i + '_progress').progressBar(parseFloat(json_data["Hardware"]["HDD"][i]["Sensors"]["Load"]["Used Space"]["Value"]).toFixed(1));
                } catch {

                }
            }
        }
    });

    if (firstTime) {
        firstTime = false;

        $.ajaxSetup({
            async: true
        });
    }
}

var cpuDropdownBuilded = false;
var cpuId = undefined;

function BuildCpuCard() {
    if (cpuId == undefined) {
        var cpu_id = 0;
    } else {
        cpu_id = cpuId;
    }

    if (json_data["Hardware"]["CPU"].length > 1) {
        document.getElementById("cpu_dropdown").classList.remove("dropdown-hide");
        document.getElementById("cpu_id").classList.add("dropdown-toggle");
        document.getElementById("cpu_id").innerHTML = "CPU#" + cpu_id;
        document.getElementById("cpu_id").setAttribute("style", "");
        document.getElementById("cpu_list").hidden = false;
    } else {
        document.getElementById("cpu_dropdown").classList.remove("dropdown-toggle");
        document.getElementById("cpu_id").classList.add("dropdown-hide");
        document.getElementById("cpu_id").innerHTML = "CPU";
        document.getElementById("cpu_id").setAttribute("style", "cursor: default");
        document.getElementById("cpu_list").hidden = true;
    }

    if (!cpuDropdownBuilded) {
        document.getElementById("cpu_list").innerHTML = "";

        for (let i = 0; i < json_data["Hardware"]["CPU"].length; i++) {
            var dropdown = document.getElementById("cpu_list");

            var item = document.createElement("a");
            item.setAttribute("class", "dropdown-item");
            item.setAttribute("onclick", "SetCurrentCPU(this)");
            item.setAttribute("href", "#");
            item.innerText = "CPU#" + i;

            dropdown.appendChild(item);
            if (i < json_data["Hardware"]["CPU"].length - 1) {
                var divider = document.createElement("div");
                divider.setAttribute("class", "dropdown-divider");

                dropdown.appendChild(divider);
            }
        }
        cpuDropdownBuilded = true;
    }

    try {
        var cpu_model = json_data["Hardware"]["CPU"][cpu_id]["Name"];
        var cpu_load = parseFloat(json_data["Hardware"]["CPU"][cpu_id]["Sensors"]["Load"]["CPU Total"]["Value"]).toFixed(1);
        if (json_data["Hardware"]["CPU"][cpu_id]["Sensors"]["Temperature"]["CPU Package"] == undefined) {
            var cpu_temperature = parseFloat(json_data["Hardware"]["CPU"][cpu_id]["Sensors"]["Temperature"]["Average"]).toFixed(1);
        } else {
            var cpu_temperature = parseFloat(json_data["Hardware"]["CPU"][cpu_id]["Sensors"]["Temperature"]["CPU Package"]["Value"]).toFixed(1);
        }
        var cpu_max_temperature = parseFloat(json_data["Hardware"]["CPU"][cpu_id]["Sensors"]["Temperature"]["Maximum"]).toFixed(1);
        var cpu_clock = parseFloat(json_data["Hardware"]["CPU"][cpu_id]["Sensors"]["Clock"]["Average"]).toFixed(1);
        var cpu_max_clock = parseFloat(json_data["Hardware"]["CPU"][cpu_id]["Sensors"]["Clock"]["Maximum"]).toFixed(1);
        var cpu_power = parseFloat(json_data["Hardware"]["CPU"][cpu_id]["Sensors"]["Power"]["CPU Package"]["Value"]).toFixed(1);
        var cpu_max_power = parseFloat(json_data["Hardware"]["CPU"][cpu_id]["Sensors"]["Power"]["Maximum"]).toFixed(1);
    } catch {

    }

    document.getElementById("cpu_model").innerHTML = cpu_model;
    document.getElementById("cpu_temperature_value").innerHTML = cpu_temperature + " °C";
    document.getElementById("cpu_clock_value").innerHTML = cpu_clock + " MHz";
    if (cpu_power == undefined) {
        document.getElementById("cpu_power_value").innerHTML = "-";
        $('#cpu_power').progressBar(0);
    } else {
        document.getElementById("cpu_power_value").innerHTML = cpu_power + " W";
        $('#cpu_power').progressBar(parseFloat(mapValue(cpu_power, [0, cpu_max_power], [0, 100])));
    }

    $('#cpu_temperature').progressBar(parseFloat(mapValue(cpu_temperature, [0, cpu_max_temperature], [0, 100])));
    $('#cpu_clock').progressBar(parseFloat(mapValue(cpu_clock, [0, cpu_max_clock], [0, 100])));

    var cpu_chart = window.chart = $('#cpu_load').data('easyPieChart');
    cpu_chart.update(cpu_load);
}

function SetCurrentCPU(element) {
    var id = parseInt(element.innerHTML.split('#')[1]);

    cpuId = id;
    BuildCpuCard();
}

function arrayContainsArray(superset, subset) {
    return subset.every(function (value) {
        return (superset.indexOf(value) >= 0);
    });
}

function arraysEqual(a, b) {
    if (a === b) return true;
    if (a == null || b == null) return false;
    if (a.length != b.length) return false;

    // If you don't care about the order of the elements inside
    // the array, you should sort both arrays here.
    // Please note that calling sort on an array will modify that array.
    // you might want to clone your array first.
    a = a.sort();
    b = b.sort();

    for (var i = 0; i < a.length; ++i) {
        if (a[i] !== b[i]) return false;
    }
    return true;
}

function getSecondPart(str) {
    return str.split('#')[1];
}

function mapValue(value, from, to) {
    return to[0] + (value - from[0]) * (to[1] - to[0]) / (from[1] - from[0]);
}

function DisableCard(cardType) {
    document.getElementById(`${cardType}_card`).classList.add("disabled");

    if (cardType != cardTypes.HDD) {
        document.getElementById(`${cardType}_load`).classList.add("disabled");
        $(`#${cardType}_load .donut-inner .value`)[0].innerHTML = "-";

        document.getElementById(`${cardType}_model`).classList.add("disabled");
        document.getElementById(`${cardType}_model`).innerHTML = "-";

        let chart = window.chart = $(`#${cardType}_load`).data("easyPieChart");
        chart.options.barColor = "#D4D4D4";
        chart.options.trackColor = trackColor;
        chart.update();
    }

    if (cardType == cardTypes.CPU) {
        $('#cpu_temperature').progressBar(0);
        $('#cpu_clock').progressBar(0);
        $('#cpu_power').progressBar(0);

        document.getElementById("cpu_power_value").innerHTML = "-";
        document.getElementById("cpu_temperature_value").innerHTML = "-";
        document.getElementById("cpu_clock_value").innerHTML = "-";
    }
    else if (cardType == cardTypes.GPU) {
        $('#gpu_memory_load').progressBar(0);
        $('#gpu_temperature').progressBar(0);
        $('#gpu_core_clock').progressBar(0);
        $('#gpu_memory_clock').progressBar(0);

        document.getElementById("gpu_memory_load_value").innerHTML = "-";
        document.getElementById("gpu_temperature_value").innerHTML = "-";
        document.getElementById("gpu_core_clock_value").innerHTML = "-";
        document.getElementById("gpu_memory_clock_value").innerHTML = "-";
    }
    else if (cardType == cardTypes.RAM) {
        document.getElementById("total_ram").innerHTML = "Total RAM: -";
        document.getElementById("free_ram").innerHTML = "Free RAM: -";
        document.getElementById("used_ram").innerHTML = "Used RAM: -";
    }
    else if (cardType == cardTypes.HDD) {
        document.getElementById("hdd_content").innerHTML = "";
    }
}

function EnableCard(cardType) {
    document.getElementById(`${cardType}_card`).classList.remove("disabled");

    if (cardType != cardTypes.HDD) {
        document.getElementById(`${cardType}_load`).classList.remove("disabled");
        document.getElementById(`${cardType}_model`).classList.remove("disabled");

        let chart = window.chart = $(`#${cardType}_load`).data("easyPieChart");
        chart.options.barColor = function (percent) {
            return perc2color(percent, 100, 0);
        }
    }
}

function IsEnabled(cardType) {
    if (!document.getElementById(`${cardType}_card`).classList.contains("disabled")) {
        return true;
    }
    return false;
}

const cardTypes = {
    CPU: "cpu",
    GPU: "gpu",
    RAM: "ram",
    HDD: "hdd"
}