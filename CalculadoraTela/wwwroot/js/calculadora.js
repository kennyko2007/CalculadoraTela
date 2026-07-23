document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("calcForm");
    if (!form) return;

    const inputs = form.querySelectorAll(".calc-input, #TipoProducto");

    inputs.forEach(input => {
        input.addEventListener("input", calcularEnTiempoReal);
        input.addEventListener("change", calcularEnTiempoReal);
    });

    function calcularEnTiempoReal() {
        const formData = new FormData(form);
        const data = {};
        
        formData.forEach((value, key) => {
            // Normaliza las comas por puntos en los valores numéricos por si acaso
            data[key] = typeof value === 'string' ? value.replace(',', '.') : value;
        });

        fetch('/Home/CalcularAjax', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify(data)
        })
        .then(response => response.json())
        .then(res => {
            if (res.success && res.data) {
                const model = res.data;

                // Matriz superior
                safeSetText("resUrdimbreRes", (model.resistenciaUrdimbre || 0).toFixed(2) + " KgF");
                safeSetText("resUrdimbrePeso", (model.pesoUrdimbre || 0).toFixed(2) + " gr");
                safeSetText("resUrdimbrePorc", (model.porcentajeUrdimbre || 0).toFixed(1) + " %");

                safeSetText("resTramaRes", (model.resistenciaTrama || 0).toFixed(2) + " KgF");
                safeSetText("resTramaPeso", (model.pesoTrama || 0).toFixed(2) + " gr");
                safeSetText("resTramaPorc", (model.porcentajeTrama || 0).toFixed(1) + " %");

                safeSetText("resUrdRefRes", (model.urdimbreRefuerzoResistencia || 0).toFixed(2) + " KgF");
                
                // Cantidad de Conos (REDONDEA.PAR)
                safeSetText("resCantidadConos", model.maquinaNumero || 0);
                
                safeSetText("resPesoMetro", (model.pesoMetroLineal || 0).toFixed(1) + " gml");

                // Bloque de Resultados Inferior
                safeSetText("lblTipoProductoRes", model.tipoProducto || "");
                safeSetText("resMedida", `${model.ancho || 0} x ${model.corte || 0}`);
                safeSetText("resTejidoConcatenado", `${model.urdimbreTejido || 0}x${model.tramaTejido || 0}`);
                safeSetText("resDenierConcatenado", `${model.urdimbreDenier || 0}x${model.tramaDenier || 0}`);

                safeSetText("resUrdimbreRes2", (model.resistenciaUrdimbre || 0).toFixed(2));
                safeSetText("resUrdimbrePorc2", (model.porcentajeUrdimbre || 0).toFixed(0));
                
                safeSetText("resTramaRes2", (model.resistenciaTrama || 0).toFixed(2));
                safeSetText("resTramaPorc2", (model.porcentajeTrama || 0).toFixed(0));

                safeSetText("resUrdRefRes2", (model.urdimbreRefuerzoResistencia || 0).toFixed(2));
                safeSetText("resAnchoRefuerzoLabel", model.anchoRefuerzoFactor || 0);
                
                safeSetText("resPesoBase", (model.pesoTejidoBase || 0).toFixed(1));
                safeSetText("resPesoLaminado", (model.pesoConLaminado || 0).toFixed(1));
                safeSetText("resPesoRefuerzo", (model.pesoConRefuerzo || 0).toFixed(1)); // GMP
                safeSetText("resPesoMetro2", (model.pesoMetroLineal || 0).toFixed(1)); // GML
            }
        })
        .catch(error => console.error("Error en el cálculo AJAX:", error));
    }

    // Función auxiliar para evitar errores si falta algún elemento DOM
    function safeSetText(elementId, text) {
        const el = document.getElementById(elementId);
        if (el) el.innerText = text;
    }

    // Botón Guardar en Historial por AJAX
    const btnGuardar = document.getElementById("btnGuardar");
    if (btnGuardar) {
        btnGuardar.addEventListener("click", function () {
            const formData = new FormData(form);
            const data = {};
            formData.forEach((value, key) => { 
                data[key] = typeof value === 'string' ? value.replace(',', '.') : value; 
            });

            fetch('/Home/GuardarHistorial', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify(data)
            })
            .then(response => response.json())
            .then(res => {
                if (res.success) {
                    alert("¡Registro guardado correctamente en la base de datos!");
                    window.location.href = '/Home/Historial';
                } else {
                    alert("Error al guardar: " + (res.message || "Desconocido"));
                }
            })
            .catch(error => console.error("Error al guardar:", error));
        });
    }
});
