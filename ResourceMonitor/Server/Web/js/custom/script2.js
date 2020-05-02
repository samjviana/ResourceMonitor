var firstTime = true;
var json_data = undefined;

function isMobile() {
    try {
        document.createEvent("TouchEvent");
        return true;
    } catch (exception) {
        return false
    }
}

$(document).ready(function () {
    $("#sidebar_toggle").on("click", function () {
        $("#sidebar").toggleClass("active");
        $('#content').toggleClass('moved');
        if(isMobile()) {
            $('#sidebar_toggle').removeClass("not_mobile");
        } else {
            $('#sidebar_toggle').addClass("not_mobile");
        }
        if($('#sidebar_toggle').hasClass("fa-arrow-right")) {
            $('#sidebar_toggle').removeClass("fa-arrow-right");
            $('#sidebar_toggle').addClass("fa-arrow-left");
        } else {
            $('#sidebar_toggle').removeClass("fa-arrow-left");
            $('#sidebar_toggle').addClass("fa-arrow-right");
        }
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
            },
            swipeLeft: function (event, direction, distance, duration, fingerCount, finderData, currentDirection) {
                $("#sidebar").addClass("active");
                $('#content').removeClass('moved');
                if ($(".navbar-collapse").hasClass("navbar-collapse collapse show")) {
                    $(".navbar-toggler").click();
                }
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

$('.chart').easyPieChart({
    size: 135,
    scaleColor: false,
    trackColor: "#d4d4d4",
    lineWidth: 13,
    onStep: function (from, to, percent) {
        $(this.el).find('.value').text(Math.round(percent) + "%");
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
function BuildSideBar() {
    console.log(2);
    if (firstTime) {
        $.ajaxSetup({
            async: false
        });
    }
    console.log(3);

    $.getJSON('deviceList.json', function (data) {
        json_data = data;
        console.log(json_data);
        if (data["empty"] == "empty") {
            return;
        }
        /*
        if(data["Change"] == "False" && !firstTime) {
            return;
        }

        computerData = data;

        var computer_list = document.getElementById("computer_list");
        computer_list.innerHTML = "";

        computerList = data;

        if (firstTime) {
            for (let i = 0; i < computerList["Count"]; i++) {
                if (computerList["Computers"][i]["State"]) {
                    currentComputer = computerList["Computers"][i]["Name"];
                    break;
                }

            }
        }

        for (let i = 0; i < computerList["Count"]; i++) {
            var online = false;
            if (computerList["Computers"][i]["State"]) {
                online = true;
            }

            var listElement = document.createElement("li");

            var onOffDiv = document.createElement("div");
            var onOffIcon = document.createElement("i");
            onOffDiv.setAttribute("class", "pl-1 pr-2");
            if (online) {
                onOffIcon.setAttribute("class", "fas fa-circle green-text");
            } else {
                onOffIcon.setAttribute("class", "fas fa-circle red-text");
            }

            onOffDiv.appendChild(onOffIcon);

            var anchorElement = document.createElement("a");


            anchorElement.appendChild(onOffDiv);
            anchorElement.appendChild(document.createTextNode(computerList["Computers"][i]["Name"]));
            anchorElement.setAttribute("id", computerList["Computers"][i]["Name"]);
            anchorElement.setAttribute("onclick", "SetCurrentComputer(this)");
            anchorElement.setAttribute("class", "d-flex flex-row pl-2 pr-2");
            listElement.appendChild(anchorElement);

            var sidebar = document.getElementById("computer_list");
            var find = document.getElementById(computerList["Computers"][i]["Name"]);
            if (find == null) {
                sidebar.appendChild(listElement);
            }

            try {
                document.getElementById(currentComputer).parentElement.classList.add("active");
            } catch {

            }
        }*/
    });
}

function SetCurrentComputer(element) {
    if (!element.parentElement.classList.contains("offline")) {
        var previousComputer = sidebar.getElementsByClassName("active")[0].firstElementChild;

        sidebar.getElementsByClassName("active")[0].classList.remove("active");

        element.parentElement.classList.add("active");

        currentComputer = element.getAttribute("id");

        if (currentComputer != previousComputer.getAttribute("id")) {
            cpuDropdownBuilded = false;
            Loading();
        }
    }
}

function Loaded() {
    $('#preloader .inner').fadeOut();
    $('#preloader').delay(0).fadeOut('slow');
    $('body').delay(250).css({ 'overflow': 'visible' });
}

function Loading() {
    $("#preloader .inner").fadeIn();
    $("body").delay(250).css({ "overflow": "hidden" });
    $("#preloader").delay(250).fadeIn(function () {
        setTimeout(LoadData, 750);

        setTimeout(Loaded, 000); // 2000
    });
}

function LoadData() {
    console.log(1);
    BuildSideBar();
    console.log(10);

    /*
    if (currentComputer == undefined) {
        return;
    }

    $.getJSON(currentComputer + ".json", function (data) {
        json_data = data;
        if (currentComputer == "PC-SAMUEL") {
            json_data["Hardware"]["CPU"][1] = extraCpu;
        }
        /*
         * CPU
         *
        BuildCpuCard();
        /*
         * GPU
         *
        if (json_data["Hardware"]["GpuNvidia"].length != 0) {
            var gpu_type = "GpuNvidia";
        } else if (json_data["Hardware"]["GpuAti"].length != 0) {
            var gpu_type = "GpuAti";
        } else {
            var gpu_chart = window.chart = $('#gpu_load').data('easyPieChart');
            gpu_chart.options.barColor = "#E1E1E1";
            gpu_chart.options.trackColor = trackColor;
            gpu_chart.update();

            $('#gpu_memory_load').progressBar(0);
            $('#gpu_temperature').progressBar(0);
            $('#gpu_core_clock').progressBar(0);
            $('#gpu_memory_clock').progressBar(0);

            document.getElementById("gpu_card").classList.add("disabled")
            document.getElementById("gpu_model").innerHTML = "-";
            document.getElementById("gpu_memory_load_value").innerHTML = "-";
            document.getElementById("gpu_temperature_value").innerHTML = "-";
            document.getElementById("gpu_core_clock_value").innerHTML = "-";
            document.getElementById("gpu_memory_clock_value").innerHTML = "-";

            var gpu_type = "not found";
        }
        if (gpu_type != "not found") {
            document.getElementById("gpu_card").classList.remove("disabled")

            var gpu_model = json_data["Hardware"][gpu_type][0]["Name"];
            var gpu_load = parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Load"]["GPU Core"]["Value"]).toFixed(1);
            var gpu_memory_load = parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Load"]["GPU Memory"]["Value"]).toFixed(1);
            var gpu_temperature = parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Temperature"]["GPU Core"]["Value"]).toFixed(1);
            var gpu_max_temperature = parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Temperature"]["Maximum"]).toFixed(1);
            var gpu_core_clock = parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Clock"]["GPU Core"]["Value"]).toFixed(1);
            var gpu_max_core_clock = (parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Clock"]["GPU Core"]["Maximum"])).toFixed(1);
            var gpu_memory_clock = parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Clock"]["GPU Memory"]["Value"]).toFixed(1);
            var gpu_max_memory_clock = (parseFloat(json_data["Hardware"][gpu_type][0]["Sensors"]["Clock"]["GPU Memory"]["Maximum"])).toFixed(1);

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
            gpu_chart.options.barColor = function (percent) {
                return perc2color(percent, 100, 0);
            }
            gpu_chart.update(gpu_load);
        }
        /*
         * RAM
         *
        var ram_model = json_data["Hardware"]["RAM"][0]["Name"];
        var ram_load = parseFloat(json_data["Hardware"]["RAM"][0]["Sensors"]["Load"]["Memory"]["Value"]).toFixed(1);
        var free_ram = parseFloat(json_data["Hardware"]["RAM"][0]["Sensors"]["Data"]["Available Memory"]["Value"]).toFixed(1);
        var used_ram = parseFloat(json_data["Hardware"]["RAM"][0]["Sensors"]["Data"]["Used Memory"]["Value"]).toFixed(1);
        var total_ram = parseFloat(json_data["Hardware"]["RAM"][0]["Sensors"]["Data"]["Total Memory"]).toFixed(1);

        document.getElementById("ram_model").innerHTML = ram_model;
        document.getElementById("total_ram").innerHTML = "Total RAM: " + total_ram + " GB";
        document.getElementById("free_ram").innerHTML = "Free RAM: " + free_ram + " GB";
        document.getElementById("used_ram").innerHTML = "Used RAM: " + used_ram + " GB";

        var ram_chart = window.chart = $('#ram_load').data('easyPieChart');
        ram_chart.update(ram_load);
        /*
         * HDD
         *
        for (let i = 0; i < json_data["Hardware"]["HDD"].length; i++) {
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
                disk.innerHTML = "";
                storage.innerHTML = parseFloat(json_data["Hardware"]["HDD"][i]["Sensors"]["Load"]["Used Space"]["Value"]).toFixed(1) + " %";
                $('#hdd' + i + '_progress').progressBar(parseFloat(json_data["Hardware"]["HDD"][i]["Sensors"]["Load"]["Used Space"]["Value"]).toFixed(1));
            } catch {

            }
        }
    });*/

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
        document.getElementById("cpu_id").innerHTML = "CPU#" + cpu_id;
        document.getElementById("cpu_id").setAttribute("style", "");
        document.getElementById("cpu_list").hidden = false;
    } else {
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
        if(json_data["Hardware"]["CPU"][cpu_id]["Sensors"]["Temperature"]["CPU Package"] == undefined) {
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
    if(cpu_power == undefined) {
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

function perc2color(perc, min, max) {
    if (perc >= 100) {
        return "#FF0000";
    } else if (perc <= 0) {
        return "#00FF00";
    }

    var base = (max - min);

    if (base == 0) { perc = 100; }
    else {
        perc = (perc - min) / base * 100;
    }
    var r, g, b = 0;
    if (perc < 50) {
        r = 255;
        g = Math.round(5.1 * perc);
    }
    else {
        g = 255;
        r = Math.round(510 - 5.10 * perc);
    }
    var h = r * 0x10000 + g * 0x100 + b * 0x1;
    return '#' + ('000000' + h.toString(16)).slice(-6);
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
