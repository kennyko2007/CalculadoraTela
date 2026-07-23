document.addEventListener("DOMContentLoaded", function () {
    const inputs = document.querySelectorAll(".calc-input");

    inputs.forEach(input => {
        input.addEventListener("input", calcularEnTiempoReal);
        input.addEventListener("change", calcularEnTiempoReal);
    });

    const btnGuardar = document.getElementById("btnGuardar");
    if (btnGuardar) {
        btnGuardar.addEventListener("click", function (e) {
            e.preventDefault();
            enviarFormulario();
        });
    }
});

function calcularEnTiempoReal() {
    // 1. Obtener valores de entrada
    const tipoProducto = document.getElementById("TipoProducto").value;
    const ancho = parseFloat(document.getElementById("Ancho").value) || 0;
    const corte = parseFloat(document.getElementById("Corte").value) || 0;
    const laminado = parseFloat(document.getElementById("Laminado").value) || 0;

    const urdimbreTejido = parseFloat(document.getElementById("UrdimbreTejido").value) || 0;
    const cintaUrdimbre = parseFloat(document.getElementById("CintaUrdimbre").value) || 0;
    const urdimbreDenier = parseFloat(document.getElementById("UrdimbreDenier").value) || 0;

    const tramaTejido = parseFloat(document.getElementById("TramaTejido").value) || 0;
    const cintaTrama = parseFloat(document.getElementById("CintaTrama").value) || 0;
    const tramaDenier = parseFloat(document.getElementById("TramaDenier").value) || 0;

    const urdimbreRefuerzoTejido = parseFloat(document.getElementById("UrdimbreRefuerzoTejido").value) || 0;
    const cintaRefuerzo = parseFloat(document.getElementById("CintaRefuerzo").value) || 0;
    const denierRefuerzo = parseFloat(document.getElementById("DenierRefuerzo").value) || 0;
    
    let anchoRefuerzoFactor = parseInt(document.getElementById("AnchoRefuerzoFactor").value) || 0;
    if (anchoRefuerzoFactor < 0) anchoRefuerzoFactor = 0;
    if (anchoRefuerzoFactor > 12) anchoRefuerzoFactor = 12;

    // 2. Cantidad de Conos / Máquina
    let factorTipoProducto = tipoProducto.toLowerCase() === "tubular" ? 2.0 : 1.0;
    let maquinaNumero = 18;
    if (cintaUrdimbre > 0) {
        let valorConos = (ancho * (factorTipoProducto * 10.0)) / (cintaUrdimbre / 1.035);
        let parInferior = Math.ceil(valorConos);
        if (parInferior % 2 !== 0) parInferior++;
        maquinaNumero = parInferior;
    }

    // 3. Resistencias
    let resistenciaUrdimbre = (urdimbreTejido * 2.0) * (urdimbreDenier / 1000.0) * 4.7;
    let resistenciaTrama = (tramaTejido * 2.0) * (tramaDenier / 1000.0) * 4.7;
    let urdimbreRefuerzoResistencia = (urdimbreRefuerzoTejido * 2.0) * (denierRefuerzo / 1000.0) * 4.7;

    // 4. Pesos Base
    let pesoUrdimbre = (urdimbreTejido * urdimbreDenier) / 228.6;
    let pesoTrama = (tramaTejido * tramaDenier) / 228.6;

    // 5. Porcentajes
    let sumaPesosBase = pesoUrdimbre + pesoTrama;
    let porcUrdimbre = sumaPesosBase > 0 ? (pesoUrdimbre * 100.0) / sumaPesosBase : 0;
    let porcTrama = sumaPesosBase > 0 ? (pesoTrama * 100.0) / sumaPesosBase : 0;

    // 6. GM2, Laminado y Refuerzo
    let pesoTejidoBase = sumaPesosBase * 1.05;
    let pesoConLaminado = pesoTejidoBase + laminado;

    const factoresRefuerzo = [1.00, 1.01, 1.02, 1.03, 1.04, 1.05, 1.06, 1.07, 1.08, 1.09, 1.10, 1.11, 1.12];
    let factorTablaRefuerzo = factoresRefuerzo[anchoRefuerzoFactor] || 1.00;
    let pesoConRefuerzo = factorTablaRefuerzo * pesoConLaminado;

    // 7. GML (Peso Metro Lineal)
    let pesoMetroLineal = pesoConRefuerzo * (ancho / 100.0);

    // --- ACTUALIZAR DOM (VISTA) ---
    document.getElementById("resUrdimbreRes").innerText = resistenciaUrdimbre.toFixed(2) + " KgF";
    document.getElementById("resUrdimbrePeso").innerText = pesoUrdimbre.toFixed(2) + " gr";
    document.getElementById("resUrdimbrePorc").innerText = porcUrdimbre.toFixed(1) + " %";

    document.getElementById("resTramaRes").innerText = resistenciaTrama.toFixed(2) + " KgF";
    document.getElementById("resTramaPeso").innerText = pesoTrama.toFixed(2) + " gr";
    document.getElementById("resTramaPorc").innerText = porcTrama.toFixed(1) + " %";

    document.getElementById("resUrdRefRes").innerText = urdimbreRefuerzoResistencia.toFixed(2) + " KgF";
    document.getElementById("resAnchoRefuerzoLabel").innerText = anchoRefuerzoFactor + " cm";

    document.getElementById("resCantidadConos").innerText = maquinaNumero;
    document.getElementById("resPesoMetro").innerText = pesoMetroLineal.toFixed(1) + " gml";

    // Bloque Inferior de Resultados
    document.getElementById("lblTipoProductoRes").innerText = tipoProducto;
    document.getElementById("resMedida").innerText = `${ancho}x${corte}`;
    document.getElementById("resTejidoConcatenado").innerText = `${urdimbreTejido}x${tramaTejido}`;
    document.getElementById("resDenierConcatenado").innerText = `${urdimbreDenier}x${tramaDenier}`;

    document.getElementById("resUrdimbreRes2").innerText = resistenciaUrdimbre.toFixed(2);
    document.getElementById("resUrdimbrePorc2").innerText = porcUrdimbre.toFixed(0);
    document.getElementById("resTramaRes2").innerText = resistenciaTrama.toFixed(2);
    document.getElementById("resTramaPorc2").innerText = porcTrama.toFixed(0);
    document.getElementById("resUrdRefRes2").innerText = urdimbreRefuerzoResistencia.toFixed(2);

    document.getElementById("resPesoBase").innerText = pesoTejidoBase.toFixed(1);
    document.getElementById("resPesoLaminado").innerText = pesoConLaminado.toFixed(1);
    document.getElementById("resPesoRefuerzo").innerText = pesoConRefuerzo.toFixed(1);
    document.getElementById("resPesoMetro2").innerText = pesoMetroLineal.toFixed(1);
}

function enviarFormulario() {
    const form = document.getElementById("calcForm");
    const formData = new FormData(form);

    fetch('/Home/Calcular', {
        method: 'POST',
        body: formData
    })
    .then(response => {
        if (response.ok) {
            window.location.href = '/Home/Historial';
        } else {
            alert("Error al guardar el registro.");
        }
    })
    .catch(error => console.error('Error:', error));
}
