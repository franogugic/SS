/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', 'sans-serif']
      },
      colors: {
        ink: '#14181f',
        paper: '#f8faf6',
        signal: '#0f766e',
        warning: '#b45309',
        danger: '#be123c'
      }
    }
  },
  plugins: []
};
