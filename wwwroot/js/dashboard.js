document.addEventListener("DOMContentLoaded", function () {
    // Initialize GridStack with specific options
    let grid = GridStack.init({
        cellHeight: 100,
        animate: true,
        float: true
    });
    const widgetContainer = document.getElementById("widgetContainer");

    // Define the chart widgets with their properties
    const chartWidgets = [
        { id: "membershipPieChart", w: 5, h: 5, name: "Membership Pie Chart" },
        { id: "cityBarChart", w: 5, h: 5, name: "City Bar Chart" },
        { id: "MemberJoinLineChart1", w: 5, h: 5, name: "Member Join Line Chart 1" },
        { id: "SectorPieChart", w: 5, h: 5, name: "Sector Distribution Count Pie Chart" },
        { id: "SectorBarChart", w: 5, h: 5, name: "Sector Distribution Count Bar Chart" },
        { id: "TagBarChart", w: 5, h: 5, name: "Tag Distribution Count Bar Chart" }
    ];

    // Map chart IDs to their respective initialization functions
    const chartInitializers = {
        "cityBarChart": initializeCityBarChart,
        "membershipPieChart": initializeMembershipPieChart,
        "MemberJoinLineChart1": initializeMemberJoinLineChart,
        "SectorPieChart": initializeSectorPieChart,
        "SectorBarChart": initializeSectorBarChart,
        "TagBarChart": initializeTagBarChart
    };

    // Fetch the saved layout from the server
    fetch('/Home/GetDashboardLayout')
        .then(res => res.ok ? res.json() : Promise.reject(res))
        .then(data => {
            const layoutData = JSON.parse(data.layoutData || '[]');
            // Iterate over each item in the layout data
            layoutData.forEach(item => {
                // Create a new grid item
                const newItem = document.createElement("div");
                newItem.classList.add("grid-stack-item");

                // Set only data-gs-* attributes to maintain consistency
                newItem.setAttribute("data-gs-x", item.x);
                newItem.setAttribute("data-gs-y", item.y);
                newItem.setAttribute("data-gs-width", item.width);
                newItem.setAttribute("data-gs-height", item.height);

                // Create and append the chart container
                const chartContainer = createChartContainer(item.id);
                newItem.appendChild(chartContainer);
                document.getElementById("dashboard").appendChild(newItem);

                // Initialize grid and apply the widget
                grid.makeWidget(newItem);

                // Use grid.update() to apply width and height changes
                grid.update(newItem, {
                    x: item.x,
                    y: item.y,
                    w: item.width,
                    h: item.height
                });
               
                // Initialize the chart after adding it to the grid
                setTimeout(() => initializeChart(item.id), 100);

               
            });

            // Hide selected widgets in the panel
            layoutData.forEach(ld => {
                const panelWidget = widgetContainer.querySelector(`[data-chart-id="${ld.id}"]`);
                if (panelWidget) panelWidget.style.display = "none";
            });
        })
        .catch(err => console.error("Failed to load saved layout", err));

    // Function to create a chart container with a canvas element
    function createChartContainer(chartId) {
        let container = document.createElement("div");
        container.classList.add("grid-stack-item-content");
        let canvas = document.createElement("canvas");
        canvas.id = chartId;
        container.appendChild(canvas);

        // Add context menu event listener to the canvas
        canvas.addEventListener("contextmenu", function (event) {
            event.preventDefault(); // Prevent the default context menu
            showContextMenu(event, canvas);
        });

        return container;
    }

    // Function to create a widget for each chart
    function createWidget(chart) {
        let widget = document.createElement("div");
        widget.classList.add("grid-stack-item");

        // Set the width and height from the chart definition
        widget.setAttribute("data-gs-width", chart.w);  // Use chart.w for width
        widget.setAttribute("data-gs-height", chart.h); // Use chart.h for height
        widget.setAttribute("data-chart-id", chart.id);

        // Add click handler to add the widget to the dashboard
        widget.onclick = function () { addToDashboard(this); };

        // Create content for the widget
        let content = document.createElement("div");
        content.classList.add("grid-stack-item-content");
        content.innerHTML = `<strong>${chart.name}</strong>`;
        widget.appendChild(content);

        // Create the chart container and append it to the widget
        let chartContainer = createChartContainer(chart.id);
        widget.appendChild(chartContainer);

        // Set a timeout to initialize the chart after a slight delay
        setTimeout(() => initializeChart(chart.id), 100);

        return widget;
    }


    // Function to show the context menu
    function showContextMenu(event, canvas) {
        event.preventDefault();

        const contextMenu = document.createElement("ul");
        contextMenu.classList.add("context-menu");

        let removeItem = document.createElement("li");
        removeItem.textContent = "Remove from Dashboard";
        removeItem.onclick = function () {
            removeFromDashboard(canvas);
            contextMenu.remove(); // Remove the context menu after action
        };

        let copyItem = document.createElement("li");
        copyItem.textContent = "Copy Image";
        copyItem.onclick = function () {
            copyChartImage(canvas);
            contextMenu.remove(); // Remove the context menu after action
        };

        let saveItem = document.createElement("li");
        saveItem.textContent = "Save Image";
        saveItem.onclick = function () {
            saveChartImage(canvas);
            contextMenu.remove(); // Remove the context menu after action
        };

        contextMenu.appendChild(removeItem);
        contextMenu.appendChild(copyItem);
        contextMenu.appendChild(saveItem);

        document.body.appendChild(contextMenu);
        contextMenu.style.left = `${event.pageX}px`;
        contextMenu.style.top = `${event.pageY}px`;

        document.addEventListener("click", function closeContextMenu() {
            contextMenu.remove();
            document.removeEventListener("click", closeContextMenu);
        });
    }

    // Function to add the chart to the dashboard
    function addToDashboard(el) {
        let chartId = el.getAttribute("data-chart-id");

        let newItem = document.createElement("div");
        newItem.classList.add("grid-stack-item");

        // Retrieve width and height from the clicked element's attributes
        let width = el.getAttribute("data-gs-width");
        let height = el.getAttribute("data-gs-height");

        // Set the grid-stack attributes for the new widget
        newItem.setAttribute("data-gs-width", width);
        newItem.setAttribute("data-gs-height", height);
        newItem.setAttribute("data-gs-x", 0);  // Set x position
        newItem.setAttribute("data-gs-y", 0);  // Set y position

        // Create and append the chart container
        let chartContainer = createChartContainer(chartId);
        newItem.appendChild(chartContainer);

        // Append the item to the dashboard
        document.getElementById("dashboard").appendChild(newItem);

        // Initialize the grid widget
        grid.makeWidget(newItem);

        // Update the item in the grid with correct width/height
        grid.update(newItem, {
            x: 0,  // Update x if needed
            y: 0,  // Update y if needed
            w: parseInt(width),  // Make sure to convert to integer
            h: parseInt(height)  // Make sure to convert to integer
        });

        // Re-initialize the chart after the widget has been added
        setTimeout(() => initializeChart(chartId), 100);

        // Hide the original widget from the panel
        el.style.display = "none";
    }




    // Function to remove the chart from the dashboard
    // Function to remove the chart from the dashboard and reinitialize it in the widget container
    // Function to remove the chart from the dashboard and reinitialize it in the widget container
    function removeFromDashboard(canvas) {
        let widget = canvas.closest(".grid-stack-item");
        let chartId = canvas.id;

        // Remove the widget from the GridStack instance
        grid.removeWidget(widget);

        // Create a new widget element
        let newWidget = document.createElement("div");
        newWidget.classList.add("grid-stack-item");
        newWidget.setAttribute("data-gs-width", widget.getAttribute("data-gs-width"));
        newWidget.setAttribute("data-gs-height", widget.getAttribute("data-gs-height"));
        newWidget.setAttribute("data-chart-id", chartId);

        // Create the chart container and append it to the new widget
        let chartContainer = createChartContainer(chartId);
        newWidget.appendChild(chartContainer);

        // Add click handler to add the widget back to the dashboard
        newWidget.onclick = function () { addToDashboard(this); };

        // Append the new widget to the widget container
        document.getElementById("widgetContainer").appendChild(newWidget);

        // Reinitialize the chart
        setTimeout(() => initializeChart(chartId), 100);

        console.log(`Reinitialized chart with ID: ${chartId} in the widget container`);
    }
    // Function to copy the chart image to clipboard
    function copyChartImage(canvas) {
        canvas.toBlob(function (blob) {
            const item = new ClipboardItem({ "image/png": blob });
            navigator.clipboard.write([item]).then(() => {
                alert("Chart image copied to clipboard!");
            }).catch(err => {
                console.error("Failed to copy chart image: ", err);
            });
        });
    }

    // Function to save the chart image as a PNG file
    function saveChartImage(canvas) {
        let link = document.createElement("a");
        link.href = canvas.toDataURL("image/png");
        link.download = canvas.id + ".png";
        link.click();
    }

    // Create and append the chart widgets to the widget panel
    chartWidgets.forEach(chart => {
        widgetContainer.appendChild(createWidget(chart));
    });

    // Event listener for the customize button
    document.getElementById("customizeBtn").addEventListener("click", function () {
        // Show the widget panel and hide the customize button when clicked
        document.getElementById("widgetPanel").style.display = "block";
        document.getElementById("saveLayoutBtn").style.display = "inline-block";
        this.style.display = "none";  // Hide the customize button
        document.getElementById("closeWidgetBtn").style.display = "inline-block"; // Show the close button
    });

    // Event listener for the close widget button
    document.getElementById("closeWidgetBtn").addEventListener("click", function () {
        // Hide the widget panel and the close button, show the customize button again
        document.getElementById("widgetPanel").style.display = "none";
        document.getElementById("saveLayoutBtn").style.display = "inline-block"; // Hide the save layout button
        document.getElementById("customizeBtn").style.display = "inline-block"; // Show the customize button
        this.style.display = "none"; // Hide the close button
    });

    // Event listener for the save layout button
    document.getElementById("saveLayoutBtn").addEventListener("click", function () {
        const layoutData = grid.engine.nodes.map(widget => {
            const chartId = widget.el?.querySelector("canvas")?.id ||
                widget.el?.getAttribute("data-chart-id") ||
                "unknown";

            const chartDetails = {
                id: chartId,
                x: widget.x,
                y: widget.y,
                width: widget.w,
                height: widget.h
            };

            // Log the details of the chart widget being saved
            console.log("Saving widget layout:", chartDetails);

            return chartDetails;
        });

        // Save the layout data to the server
        fetch('/Home/SaveDashboardLayout', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                layoutData: JSON.stringify(layoutData)
            })
        })
            .then(res => res.ok ? res.json() : Promise.reject(res))
            .then(() => {
                console.log("Layout saved!");
                document.getElementById("widgetPanel").style.display = "none";
                document.getElementById("customizeBtn").style.display = "inline-block"; // Show customize button
                this.style.display = "none"; // Hide the save button after saving
            })
            .catch(err => console.error("Failed to save layout", err));
    });

    // Function to initialize the chart based on its ID
    function initializeChart(chartId) {
        const canvas = document.getElementById(chartId);

        const initFunction = chartInitializers[chartId];
        if (initFunction) {
            initFunction(chartId);
        } else {
            console.error(`No initializer found for chart ID: ${chartId}`);
        }
    }

    // Function to generate a random color
    function getRandomColor() {
        const hue = Math.floor(Math.random() * 360);
        return `hsl(${hue}, 100%, 60%)`;
    }

    // Function to initialize the City Bar Chart
    function initializeCityBarChart(chartId) {
        if (!window.CityCounts) {
            console.error("CityCounts data is missing or not in the expected format.");
            return;
        }

        const labels = cityCounts.map(item => item.city);
        const data = cityCounts.map(item => item.count);
        const backgroundColors = data.map(() => getRandomColor());

        const canvas = document.getElementById(chartId);
        const ctx = canvas.getContext("2d");

        // Destroy existing chart instance if it exists
        if (canvas.__chartInstance) {
            canvas.__chartInstance.destroy();
            canvas.__chartInstance = null; // Clear the reference
        }

        // Create a new chart instance
        canvas.__chartInstance = new Chart(ctx, {
            type: "bar",
            data: {
                labels,
                datasets: [{
                    label: "Members in Each City",
                    data,
                    backgroundColor: backgroundColors,
                    borderColor: backgroundColors,
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    tooltip: { enabled: true },
                    legend: { display: true, position: "bottom" },
                    title: {
                        display: true,
                        text: 'Members In Each City Distribution', // The chart title
                        font: {
                            size: 16
                        },
                        padding: {
                            top: 10,
                            bottom: 10
                        }
                    }
                }
            }
        });
    }

    // Function to initialize the Membership Pie Chart
    function initializeMembershipPieChart(chartId) {
        if (!window.MembershipCount) {
            console.error("MembershipCount data is missing or not available.");
            return;
        }

        const labels = MembershipCount.map(item => item.membershipType.typeName);
        const data = MembershipCount.map(item => item.count);
        const backgroundColors = data.map(() => getRandomColor());

        const canvas = document.getElementById(chartId);
        const ctx = canvas.getContext("2d");

        // Destroy existing chart instance if it exists
        if (canvas.__chartInstance) {
            canvas.__chartInstance.destroy();
            canvas.__chartInstance = null; // Clear the reference
        }

        // Create a new chart instance
        canvas.__chartInstance = new Chart(ctx, {
            type: "doughnut",
            data: {
                labels,
                datasets: [{
                    label: "Memberships Count",
                    data,
                    backgroundColor: backgroundColors,
                    borderColor: backgroundColors,
                    borderWidth: 1,
                    spacing: 5
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: "70%",
                hoverOffset: 20,
                plugins: {
                    tooltip: { enabled: true },
                    legend: { position: "right" },
                    title: {
                        display: true,
                        text: 'Memberships Distribution', // The chart title
                        font: {
                            size: 18
                        },
                        padding: {
                            top: 10,
                            bottom: 10
                        }
                    }
                }
            }
        });
    }

    // Function to initialize the Member Join Line Chart
    function initializeMemberJoinLineChart(chartId) {
        if (!window.MembersJoins) {
            console.error("MembersJoining data is missing or not available.");
            return;
        }

        const labels = MembersJoins.map(item => item.month);
        const data = MembersJoins.map(item => item.count);

        const canvas = document.getElementById(chartId);
        const ctx = canvas.getContext("2d");

        // Destroy existing chart instance if it exists
        if (canvas.__chartInstance) {
            canvas.__chartInstance.destroy();
            canvas.__chartInstance = null; // Clear the reference
        }

        // Create a new chart instance
        canvas.__chartInstance = new Chart(ctx, {
            type: "line",
            data: {
                labels,
                datasets: [{
                    label: "Member Joining",
                    data: data,
                    borderWidth: 2,
                    tension: 0.8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                animation: {
                    duration: 1000,
                    easing: "easeInOutQuad"
                },
                plugins: {
                    tooltip: { enabled: true },
                    legend: {
                        display: true,
                        position: "bottom"
                    },
                    title: {
                        display: true,
                        text: 'Member Joining Distribution', // The chart title
                        font: {
                            size: 16
                        },
                        padding: {
                            top: 10,
                            bottom: 10
                        }
                    }
                }
            }
        });
    }

    // Function to initialize the Sector Pie Chart
    function initializeSectorPieChart(chartId) {
        if (!window.SectorCounts) {
            console.error("Sector data is missing or not in the expected format.");
            return;
        }

        // Extract labels (Sector Names) and data (Counts) from the passed sector data
        const labels = SectorCounts.map(item => item.sectorName);  // Sector names
        const data = SectorCounts.map(item => item.count);  // Counts per sector

        // Generate random background colors for each sector
        const backgroundColors = data.map(() => getRandomColor());  // Random colors for each sector

        // Get the context for the chart
        const canvas = document.getElementById(chartId);
        const ctx = canvas.getContext("2d");

        // Destroy existing chart instance if it exists
        if (canvas.__chartInstance) {
            canvas.__chartInstance.destroy();
            canvas.__chartInstance = null; // Clear the reference
        }

        // Create a new chart instance
        canvas.__chartInstance = new Chart(ctx, {
            type: "doughnut",  // Doughnut chart type (can also use 'pie' if preferred)
            data: {
                labels: labels,
                datasets: [{
                    label: "Members per Sector",
                    data: data,
                    backgroundColor: backgroundColors,
                    borderColor: backgroundColors,
                    borderWidth: 1,
                    spacing: 5
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                cutoutPercentage: 70,  // Controls the 'hole' size of the doughnut
                hoverOffset: 20,  // Spacing when hovered over a slice
                plugins: {
                    tooltip: {
                        enabled: true  // Enable tooltips for better interaction
                    },
                    legend: {
                        position: "right"  // Position the legend to the right
                    },
                    title: {
                        display: true,
                        text: 'Member Per Sector Distribution', // The chart title
                        font: {
                            size: 16
                        },
                        padding: {
                            top: 10,
                            bottom: 10
                        }
                    }
                }
            }
        });
    }

    // Function to initialize the Sector Bar Chart
    function initializeSectorBarChart(chartId) {
        if (!window.SectorCounts) {
            console.error("Sector data is missing or not in the expected format.");
            return;
        }

        // Extract labels (Sector Names) and data (Counts) from the passed sector data
        const labels = SectorCounts.map(item => item.sectorName);  // Sector names
        const data = SectorCounts.map(item => item.count);  // Counts per sector
        const backgroundColors = data.map(() => getRandomColor());

        // Get the context for the chart
        const canvas = document.getElementById(chartId);
        const ctx = canvas.getContext("2d");

        // Step 1: Check if there is an existing chart instance on this canvas
        if (canvas.__chartInstance) {
            // Destroy the existing chart if it exists
            canvas.__chartInstance.destroy();
            canvas.__chartInstance = null; // Clear the reference
        }

        // Step 2: Create a new chart instance
        canvas.__chartInstance = new Chart(ctx, {
            type: "bar",  // Set the chart type to 'bar' for a bar chart
            data: {
                labels: labels,  // X-axis labels (sector names)
                datasets: [{
                    label: "Members per Sector",
                    data: data,  // Y-axis data (counts)
                    backgroundColor: backgroundColors,  // Random color for the bars
                    borderWidth: 1,  // Bar border width
                    hoverBackgroundColor: backgroundColors,  // Color when hovered over
                    hoverBorderColor: backgroundColors,  // Border color when hovered over
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    tooltip: {
                        enabled: true  // Enable tooltips for better interaction
                    },
                    legend: {
                        position: "top"  // Position the legend at the top (or "bottom", "right")
                    },
                    title: {
                        display: true,
                        text: 'Member Per Sector Distribution',  // Chart title
                        font: {
                            size: 16
                        },
                        padding: {
                            top: 10,
                            bottom: 10
                        }
                    }
                },
                scales: {
                    x: {
                        title: {
                            display: true,
                            text: 'Sectors'  // Label for X-axis
                        },
                        grid: {
                            display: false  // Hide gridlines for the X-axis
                        }
                    },
                    y: {
                        title: {
                            display: true,
                            text: 'Member Count'  // Label for Y-axis
                        },
                        beginAtZero: true,  // Start Y-axis from zero
                        ticks: {
                            stepSize: 1  // Adjust the step size of Y-axis ticks (optional)
                        }
                    }
                }
            }
        });

        console.log(`Initialized new chart with ID: ${chartId}`);
    }

    // Function to initialize the Tag Bar Chart
    function initializeTagBarChart(chartId) {
        if (!window.TagCounts) {
            console.error("Tag data is missing or not in the expected format.");
            return;
        }

        // Extract labels (Tag Names) and data (Counts) from the passed tag data
        const labels = TagCounts.map(item => item.tagName);  // Tag names
        const data = TagCounts.map(item => item.count);  // Counts per tag

        // Generate random background colors for each tag
        const backgroundColors = data.map(() => getRandomColor());  // Random colors for each tag

        // Get the context for the chart
        const canvas = document.getElementById(chartId);
        const ctx = canvas.getContext("2d");

        // Step 1: Check if there is an existing chart instance on this canvas
        if (canvas.__chartInstance) {
            // Destroy the existing chart if it exists
            canvas.__chartInstance.destroy();
            canvas.__chartInstance = null; // Clear the reference
        }

        // Step 2: Create a new chart instance
        canvas.__chartInstance = new Chart(ctx, {
            type: "bar",  // Set the chart type to 'bar' for a bar chart
            data: {
                labels: labels,  // X-axis labels (tag names)
                datasets: [{
                    label: "Members per Tag",
                    data: data,  // Y-axis data (counts)
                    backgroundColor: backgroundColors,  // Random color for the bars
                    borderWidth: 1,  // Bar border width
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    tooltip: {
                        enabled: true  // Enable tooltips for better interaction
                    },
                    legend: {
                        position: "top"  // Position the legend at the top (or "bottom", "right")
                    },
                    title: {
                        display: true,
                        text: 'Member Per Tag Distribution',  // Chart title
                        font: {
                            size: 16
                        },
                        padding: {
                            top: 10,
                            bottom: 10
                        }
                    }
                },
                scales: {
                    x: {
                        title: {
                            display: true,
                            text: 'Tags'  // Label for X-axis
                        },
                        grid: {
                            display: false  // Hide gridlines for the X-axis
                        }
                    },
                    y: {
                        title: {
                            display: true,
                            text: 'Member Count'  // Label for Y-axis
                        },
                        beginAtZero: true,  // Start Y-axis from zero
                        ticks: {
                            stepSize: 1  // Adjust the step size of Y-axis ticks (optional)
                        }
                    }
                }
            }
        });

        console.log(`Initialized new chart with ID: ${chartId}`);
    }
});
