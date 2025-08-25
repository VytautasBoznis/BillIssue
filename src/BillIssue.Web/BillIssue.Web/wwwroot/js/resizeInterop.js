window.resizeFunctions = {
    addresizeListener: function (dotNetReference) {
        const resizeHandler = () => {
            // Pass the scroll position back to the Blazor component
            dotNetReference.invokeMethodAsync('OnWindowResize', window.innerWidth);
        };
        
        // Add the scroll event listener
        window.addEventListener("resize", resizeHandler);

        // Return the scroll handler function for detaching later
        return resizeHandler;
    },
    detachScrollListener: function () {
        // Remove the scroll event listener
        window.removeEventListener("scroll", resizeHandler);
    }
};
