
function fetchProcedureAreas() {
    $.ajax({
        url: '/Grades/ProcedureAreas/GetProcedureAreas',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            console.log('Procedure Areas:', data);
          
        },
        error: function (xhr, status, error) {
            console.error('Error fetching procedure areas:', error);
        }
    })
};

function redirect() {
    window.location.href = '/Grades/ProcedureAreas/testVista'
}