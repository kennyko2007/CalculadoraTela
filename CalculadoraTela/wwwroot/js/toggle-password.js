// Alterna la visibilidad de cualquier campo de contraseña que tenga un botón
// con la clase "toggle-password" y el atributo data-target apuntando al id del input.
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".toggle-password").forEach(function (boton) {
        boton.addEventListener("click", function () {
            var targetId = boton.getAttribute("data-target");
            var input = document.getElementById(targetId);
            if (!input) return;

            var icono = boton.querySelector("i");
            var mostrando = input.type === "text";

            input.type = mostrando ? "password" : "text";

            if (icono) {
                icono.classList.toggle("bi-eye-fill", mostrando);
                icono.classList.toggle("bi-eye-slash-fill", !mostrando);
            }

            boton.setAttribute("aria-label", mostrando ? "Mostrar contraseña" : "Ocultar contraseña");
        });
    });
});
