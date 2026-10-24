import React, { createContext, useContext, useEffect, useState } from "react"

const initialState = {
    theme: "light",
    setTheme: () => null,
}

const ThemeProviderContext = createContext(initialState)

export function ThemeProvider({
    children,
    defaultTheme = "light",
    storageKey = "vite-ui-theme",
    ...props
}) {
    // SkillScape has a fixed branded light canvas. Do not restore a prior
    // system preference, because OS dark/light changes must not alter the UI.
    const [theme, setTheme] = useState(defaultTheme === "dark" ? "dark" : "light")

    useEffect(() => {
        const root = window.document.documentElement

        root.classList.remove("light", "dark")

        root.classList.add(theme)
    }, [theme])

    const value = {
        theme,
        setTheme: (theme) => {
            // Retain the explicit app appearance; never resolve through the OS.
            const nextTheme = theme === "dark" ? "dark" : "light"
            localStorage.setItem(storageKey, nextTheme)
            setTheme(nextTheme)
        },
    }

    return (
        <ThemeProviderContext.Provider {...props} value={value}>
            {children}
        </ThemeProviderContext.Provider>
    )
}

export const useTheme = () => {
    const context = useContext(ThemeProviderContext)

    if (context === undefined)
        throw new Error("useTheme must be used within a ThemeProvider")

    return context
}
