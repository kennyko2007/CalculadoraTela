document.addEventListener("DOMContentLoaded", function () {
    const inputs = document.querySelectorAll(".calc-input");
    let debounceTimer = null;

    // ── Soporte de coma como decimal (type="text" + inputmode="decimal") ──
    // Con type="text" el JS controla el valor completo: reemplazamos ","
    // por "." y eliminamos cualquier carácter no numérico.
    function sanitizarDecimal(input) {
        if (input.readOnly) return;          // campos calculados: no tocar
        let v = input.value;
        // Reemplazar coma por punto
        v = v.replace(/,/g, ".");
        // Conservar: dígitos, un único punto decimal, y "-" solo al inicio
        v = v.replace(/[^0-9.\-]/g, "");
        // Si hay más de un punto, conservar solo el primero
        const partes = v.split(".");
        if (partes.length > 2) v = partes[0] + "." + partes.slice(1).join("");
        if (input.value !== v) {
            const pos = input.selectionStart;
            input.value = v;
            // Restaurar posición del cursor
            try { input.setSelectionRange(pos, pos); } catch (_) {}
        }
    }

    inputs.forEach(input => {
        if (input.tagName === "INPUT" && !input.readOnly) {
            input.addEventListener("input", function () { sanitizarDecimal(this); });
        }
    });

    // ── Formato inteligente: decimales solo cuando no son enteros ──
    // fmt2 → máx 2 decimales; fmt1 → máx 1 decimal
    function fmt2(n) { return parseFloat(n.toFixed(2)).toString(); }
    function fmt1(n) { return parseFloat(n.toFixed(1)).toString(); }

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

    // --- Sincronización en vivo de la fila "URDIMBRE REFUERZO" ---
    // En el Excel (Hoja2) estas celdas son fórmulas, no datos de entrada:
    //   Urdimbre Refuerzo Tejido = Urdimbre Tejido x 2   (B5 = +B3*2)
    //   Denier Refuerzo          = Urdimbre Denier       (D5 = +D3)
    // Se reflejan aquí al instante (antes de que responda el servidor)
    // para que el usuario vea el mismo comportamiento que en Excel.
    const inputUrdimbreTejido = document.getElementById("UrdimbreTejido");
    const inputUrdimbreDenier = document.getElementById("UrdimbreDenier");
    const inputUrdimbreRefuerzoTejido = document.getElementById("UrdimbreRefuerzoTejido");
    const inputDenierRefuerzo = document.getElementById("DenierRefuerzo");

    function sincronizarRefuerzo() {
        const urdimbreTejido = parseFloat(inputUrdimbreTejido?.value) || 0;
        const urdimbreDenier = parseFloat(inputUrdimbreDenier?.value) || 0;
        if (inputUrdimbreRefuerzoTejido) inputUrdimbreRefuerzoTejido.value = urdimbreTejido * 2;
        if (inputDenierRefuerzo) inputDenierRefuerzo.value = urdimbreDenier;
    }

    inputUrdimbreTejido?.addEventListener("input", sincronizarRefuerzo);
    inputUrdimbreDenier?.addEventListener("input", sincronizarRefuerzo);

    // Cálculo inicial al cargar la página (por si los valores por defecto
    // del HTML difieren de los que trae el modelo)
    sincronizarRefuerzo();
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
            Corte: 100 // fijo, igual que en el Excel — ya no es un campo editable
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

            // El backend es la fuente de verdad para los campos derivados
            // (igual que las fórmulas del Excel): se sincronizan aquí por
            // si hubiera alguna diferencia con el cálculo instantáneo del cliente.
            if (inputUrdimbreRefuerzoTejido) inputUrdimbreRefuerzoTejido.value = r.urdimbreRefuerzoTejido;
            if (inputDenierRefuerzo) inputDenierRefuerzo.value = r.denierRefuerzo;

            // --- Bloque DATOS / RESISTENCIA / PESO / % (mismo orden que filas 8-12 de Hoja2) ---
            setText("lblTipoProductoRes", r.tipoProducto);
            setText("resMedida", `${r.ancho} x ${r.corte}`);
            setText("resTejidoConcatenado", `${r.urdimbreTejido} x ${r.tramaTejido}`);
            setText("resDenierConcatenado", `${r.urdimbreDenier} x ${r.tramaDenier}`);
            setText("resCantidadConos", r.maquinaNumero);

            // Fila 8: Producto / Urdimbre
            setText("resUrdimbreRes2", `${fmt2(r.resistenciaUrdimbre)} KgF`);
            setText("resPesoUrdimbreTipo", `${fmt2(r.pesoUrdimbre)} gr`);
            setText("resUrdimbrePorc2", `${fmt1(r.porcentajeUrdimbre)} %`);

            // Fila 10: Tejido / Trama
            setText("resTramaRes2", `${fmt2(r.resistenciaTrama)} KgF`);
            setText("resPesoTramaTipo", `${fmt2(r.pesoTrama)} gr`);
            setText("resTramaPorc2", `${fmt1(r.porcentajeTrama)} %`);

            // Fila 12: Conos / Refuerzo
            setText("resUrdRefRes2", `${fmt2(r.urdimbreRefuerzoResistencia)} KgF`);
            setText("resAnchoRefuerzoLabel", `Ancho: ${r.anchoRefuerzoFactor} cm`);
            setText("resPesoRefuerzoTipo", `${fmt2(r.pesoRefuerzo)} gr`);

            // --- Bloque PESO (filas 13-16 de Hoja2: gm2, gm2+lam, gmp, gml) ---
            setText("resPesoBase", fmt1(r.pesoTejidoBase));
            setText("resPesoLaminado", fmt1(r.pesoConLaminado));
            setText("resGmp", fmt1(r.pesoConRefuerzo));
            setText("resPesoMetro", fmt1(r.pesoMetroLineal));
        })
        .catch(error => {
            console.error("Error al recalcular:", error);
        });
    }

    function enviarFormulario() {
        const formData = leerFormulario();

        // Petición AJAX para guardar en la base de datos asegurando que el backend procese correctamente el GMP
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
