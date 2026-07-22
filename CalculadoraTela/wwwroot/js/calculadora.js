document.addEventListener("DOMContentLoaded", function () {
    const inputs = document.querySelectorAll(".calc-input");

    inputs.forEach(input => {
        input.addEventListener("input", recalcular);
    });

    document.getElementById("btnGuardar").addEventListener("click", guardarCalculo);
});

function recalcular() {
    const form = document.getElementById("calcForm");
    const formData = new FormData(form);

    fetch('/Home/Calcular', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            // Ficha A11
            document.getElementById("txtResumenFicha").textContent = data.resumenFicha;

            // Tabla Resistencia/Peso
            document.getElementById("resUrdimbreRes").textContent = data.resistenciaUrdimbre.toFixed(2);
            document.getElementById("resUrdimbrePeso").textContent = data.pesoUrdimbre.toFixed(2);
            document.getElementById("resUrdimbrePorc").textContent = data.porcentajeUrdimbre.toFixed(2) + "%";

            document.getElementById("resTramaRes").textContent = data.resistenciaTrama.toFixed(2);
            document.getElementById("resTramaPeso").textContent = data.pesoTrama.toFixed(2);
            document.getElementById("resTramaPorc").textContent = data.porcentajeTrama.toFixed(2) + "%";

            document.getElementById("resUrdRefRes").textContent = data.urdimbreRefuerzoResistencia.toFixed(2);

            // Pesos Totales
            document.getElementById("resPesoBase").textContent = data.pesoTejidoBase.toFixed(2);
            document.getElementById("resPesoLaminado").textContent = data.pesoConLaminado.toFixed(2);
            document.getElementById("resPesoRefuerzo").textContent = data.pesoConRefuerzo.toFixed(2);
            document.getElementById("resPesoMetro").textContent = data.pesoMetroLineal.toFixed(2);
            document.getElementById("resPesoBolsa").textContent = data.pesoPorBolsa.toFixed(2);
            document.getElementById("resProduccion").textContent = data.produccionEstimada.toFixed(2);
        })
        .catch(error => console.error("Error al recalcular:", error));
}

function guardarCalculo() {
    const form = document.getElementById("calcForm");
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
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                alert("¡Cálculo guardado con éxito!");
            }
        })
        .catch(err => console.error("Error al guardar:", err));
}