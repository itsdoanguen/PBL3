document.addEventListener('DOMContentLoaded', function () {
    if (!window.adminReportData) return;
    // User chart
    new Chart(document.getElementById('userChart'), {
        type: 'line',
        data: {
            labels: window.adminReportData.userLabels,
            datasets: [{
                label: 'User mới',
                data: window.adminReportData.userData,
                borderColor: '#007bff',
                backgroundColor: 'rgba(0,123,255,0.1)',
                fill: true,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            plugins: {legend: {display: false}},
            scales: {
                y: {
                    beginAtZero: true,
                    suggestedMax: Math.max(...window.adminReportData.userData, 1) + 2
                }
            }
        }
    });
    // Story chart
    new Chart(document.getElementById('storyChart'), {
        type: 'line',
        data: {
            labels: window.adminReportData.storyLabels,
            datasets: [{
                label: 'Truyện mới',
                data: window.adminReportData.storyData,
                borderColor: '#28a745',
                backgroundColor: 'rgba(40,167,69,0.1)',
                fill: true,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            plugins: {legend: {display: false}},
            scales: {
                y: {
                    beginAtZero: true,
                    suggestedMax: Math.max(...window.adminReportData.storyData, 1) + 2
                }
            }
        }
    });
    // Report chart
    new Chart(document.getElementById('reportChart'), {
        type: 'line',
        data: {
            labels: window.adminReportData.reportLabels,
            datasets: [{
                label: 'Báo cáo',
                data: window.adminReportData.reportData,
                borderColor: '#dc3545',
                backgroundColor: 'rgba(220,53,69,0.1)',
                fill: true,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            plugins: {legend: {display: false}},
            scales: {
                y: {
                    beginAtZero: true,
                    suggestedMax: Math.max(...window.adminReportData.reportData, 1) + 2
                }
            }
        }
    });
    // Report type chart
    new Chart(document.getElementById('reportTypeChart'), {
        type: 'bar',
        data: {
            labels: window.adminReportData.reportTypeLabels,
            datasets: [{
                label: 'Số lượng',
                data: window.adminReportData.reportTypeData,
                backgroundColor: '#ffc107',
                borderColor: '#e0a800',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {legend: {display: false}},
            scales: {
                y: {
                    beginAtZero: true,
                    suggestedMax: Math.max(...window.adminReportData.reportTypeData, 1) + 2
                }
            }
        }
    });
});
