document.addEventListener("DOMContentLoaded", function () {
    const inputs = document.querySelectorAll(".calc-input");
    let debounceTimer = null;

    // Asignar eventos a todos los inputs y selects con la clase calc-input
    inputs.forEach(input => {
        input.addEventListener("input", onInputChanged);
        input.addEventListener("change", onInputChanged);
    });

    // Botón de guardar en historial
    const btnGuardar = document.getElementById("btnGuardar");
    if (btnGuardar) {
        btnGuardar.addEventListener("click", enviarFormulario);
    }

    // Cálculo inicial al cargar la página (por si los valores por defecto
    // del HTML difieren de los que trae el modelo)
    recalcular();

    function onInputChanged() {
        // Evita disparar una petición por cada tecla presionada
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(recalcular, 150);
    }

    function leerFormulario() {
        return {
            TipoProducto: document.getElementById("TipoProducto")?.value || "Plana",
            UrdimbreTejido: parseFloat(document.getElementById("UrdimbreTejido")?.value) || 0,
            CintaUrdimbre: parseFloat(document.getElementById("CintaUrdimbre")?.value) || 0,
            UrdimbreDenier: parseFloat(document.getElementById("UrdimbreDenier")?.value) || 0,
            TramaTejido: parseFloat(document.getElementById("TramaTejido")?.value) || 0,
            CintaTrama: parseFloat(document.getElementById("CintaTrama")?.value) || 0,
            TramaDenier: parseFloat(document.getElementById("TramaDenier")?.value) || 0,
            UrdimbreRefuerzoTejido: parseFloat(document.getElementById("UrdimbreRefuerzoTejido")?.value) || 0,
            CintaRefuerzo: parseFloat(document.getElementById("CintaRefuerzo")?.value) || 0,
            DenierRefuerzo: parseFloat(document.getElementById("DenierRefuerzo")?.value) || 0,
            AnchoRefuerzoFactor: parseFloat(document.getElementById("AnchoRefuerzoFactor")?.value) || 0,
            Laminado: parseFloat(document.getElementById("Laminado")?.value) || 0,
            Ancho: parseFloat(document.getElementById("Ancho")?.value) || 0,
            Corte: parseFloat(document.getElementById("Corte")?.value) || 100
        };
    }

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    function recalcular() {
        const formData = leerFormulario();

        fetch('/Home/CalcularAjax', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(formData)
        })
        .then(response => response.json())
        .then(json => {
            if (!json || !json.success) return;
            const r = json.data;

            // --- Fila URDIMBRE ---
            setText("resUrdimbreRes", `${r.resistenciaUrdimbre.toFixed(2)} KgF`);
            setText("resUrdimbrePeso", `${r.pesoUrdimbre.toFixed(2)} gr`);
            setText("resUrdimbrePorc", `${r.porcentajeUrdimbre.toFixed(1)} %`);

            // --- Fila TRAMA ---
            setText("resTramaRes", `${r.resistenciaTrama.toFixed(2)} KgF`);
            setText("resTramaPeso", `${r.pesoTrama.toFixed(2)} gr`);
            setText("resTramaPorc", `${r.porcentajeTrama.toFixed(1)} %`);

            // --- Fila URDIMBRE REFUERZO / CANTIDAD ---
            setText("resUrdRefRes", `${r.urdimbreRefuerzoResistencia.toFixed(2)} KgF`);
            setText("resCantidadConos", r.maquinaNumero);
            setText("resAnchoRefuerzoLabel", r.anchoRefuerzoFactor);

            // --- GML (celda destacada) ---
            setText("resPesoMetro", `${r.pesoMetroLineal.toFixed(1)} gml`);
            setText("resPesoMetro2", r.pesoMetroLineal.toFixed(1));

            // --- Bloque RESULTADOS ---
            setText("lblTipoProductoRes", r.tipoProducto);
            setText("resMedida", `${r.ancho} x ${r.corte}`);
            setText("resTejidoConcatenado", `${r.urdimbreTejido} x ${r.tramaTejido}`);
            setText("resDenierConcatenado", `${r.urdimbreDenier} x ${r.tramaDenier}`);

            setText("resUrdimbreRes2", r.resistenciaUrdimbre.toFixed(2));
            setText("resUrdimbrePorc2", r.porcentajeUrdimbre.toFixed(0));
            setText("resTramaRes2", r.resistenciaTrama.toFixed(2));
            setText("resTramaPorc2", r.porcentajeTrama.toFixed(0));
            setText("resUrdRefRes2", r.urdimbreRefuerzoResistencia.toFixed(2));

            setText("resPesoBase", r.pesoTejidoBase.toFixed(1));
            setText("resPesoLaminado", r.pesoConLaminado.toFixed(1));
            setText("resPesoRefuerzo", r.pesoConRefuerzo.toFixed(1));
        })
        .catch(error => {
            console.error("Error al recalcular:", error);
        });
    }

    function enviarFormulario() {
        const formData = leerFormulario();

        // Petición AJAX para guardar en la base de datos
        fetch('/Home/GuardarHistorial', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        })
        .then(response => response.json())
        .then(data => {
            if (data && data.success) {
                alert("¡Registro guardado con éxito en el historial!");
                window.location.href = '/Home/Historial';
            } else {
                alert("Error al guardar el registro: " + (data?.message || "Verifica los datos."));
            }
        })
        .catch(error => {
            console.error("Error en la solicitud:", error);
            alert("Ocurrió un error al intentar conectar con el servidor.");
        });
    }
});
