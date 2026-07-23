document.addEventListener("DOMContentLoaded", function () {
    const inputs = document.querySelectorAll(".calc-input");

    inputs.forEach(input => {
        input.addEventListener("input", recalcular);
    });

    const btnGuardar = document.getElementById("btnGuardar");
    if (btnGuardar) {
        btnGuardar.addEventListener("click", guardarCalculo);
    }
});

function recalcular() {
    const form = document.getElementById("calcForm");
    if (!form) return;

    const formData = new FormData(form);

    fetch('/Home/Calcular', {
        method: 'POST',
        body: formData // Envía como multipart/form-data
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`Error en el servidor: ${response.status} ${response.statusText}`);
        }
        return response.json();
    })
    .then(data => {
        if (!data) return;

        // Ficha A11
        if (data.resumenFicha !== undefined) 
            document.getElementById("txtResumenFicha").textContent = data.resumenFicha;

        // Tabla Resistencia/Peso
        if (data.resistenciaUrdimbre !== undefined) 
            document.getElementById("resUrdimbreRes").textContent = Number(data.resistenciaUrdimbre).toFixed(2);
        
        if (data.pesoUrdimbre !== undefined) 
            document.getElementById("resUrdimbrePeso").textContent = Number(data.pesoUrdimbre).toFixed(2);
        
        if (data.porcentajeUrdimbre !== undefined) 
            document.getElementById("resUrdimbrePorc").textContent = Number(data.porcentajeUrdimbre).toFixed(2) + "%";

        if (data.resistenciaTrama !== undefined) 
            document.getElementById("resTramaRes").textContent = Number(data.resistenciaTrama).toFixed(2);
        
        if (data.pesoTrama !== undefined) 
            document.getElementById("resTramaPeso").textContent = Number(data.pesoTrama).toFixed(2);
        
        if (data.porcentajeTrama !== undefined) 
            document.getElementById("resTramaPorc").textContent = Number(data.porcentajeTrama).toFixed(2) + "%";

        if (data.urdimbreRefuerzoResistencia !== undefined) 
            document.getElementById("resUrdRefRes").textContent = Number(data.urdimbreRefuerzoResistencia).toFixed(2);

        // Pesos Totales
        if (data.pesoTejidoBase !== undefined) 
            document.getElementById("resPesoBase").textContent = Number(data.pesoTejidoBase).toFixed(2);
        
        if (data.pesoConLaminado !== undefined) 
            document.getElementById("resPesoLaminado").textContent = Number(data.pesoConLaminado).toFixed(2);
        
        if (data.pesoConRefuerzo !== undefined) 
            document.getElementById("resPesoRefuerzo").textContent = Number(data.pesoConRefuerzo).toFixed(2);
        
        if (data.pesoMetroLineal !== undefined) 
            document.getElementById("resPesoMetro").textContent = Number(data.pesoMetroLineal).toFixed(2);
        
        if (data.pesoPorBolsa !== undefined) 
            document.getElementById("resPesoBolsa").textContent = Number(data.pesoPorBolsa).toFixed(2);
        
        if (data.produccionEstimada !== undefined) 
            document.getElementById("resProduccion").textContent = Number(data.produccionEstimada).toFixed(2);
    })
    .catch(error => console.error("Error al recalcular:", error));
}

function guardarCalculo() {
    const form = document.getElementById("calcForm");
    if (!form) return;

    const formData = new FormData(form);
    const object = {};
    formData.forEach((value, key) => object[key] = value);

    fetch('/Home/GuardarHistorial', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(object)
    })
    .then(res => {
        if (!res.ok) {
            throw new Error(`Error al guardar: ${res.status}`);
        }
        return res.json();
    })
    .then(data => {
        if (data && data.success) {
            alert("¡Cálculo guardado con éxito!");
        } else {
            alert("No se pudo guardar el registro.");
        }
    })
    .catch(err => console.error("Error al guardar:", err));
}
