import{j as t,h as s,L as d}from"./index-DBrd4bIr.js";const m=({children:i,to:o,onClick:c,className:r="",type:u="button",disabled:e=!1})=>{const n=`
    inline-flex items-center justify-center font-bold rounded-full
    transition-all duration-300
    bg-gradient-to-r from-blue-600 to-indigo-600 text-white 
    shadow-lg shadow-blue-500/30 hover:shadow-blue-500/50 hover:-translate-y-1
    border border-white/20
    disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:translate-y-0
    tracking-wider text-sm sm:text-base
  `,l={whileHover:e?{}:{y:-4,scale:1.02},whileTap:e?{}:{scale:.98}},a=t.jsx(s.span,{className:"flex items-center gap-2",children:i});return o&&!e?t.jsx(d,{to:o,className:`inline-block w-full ${r}`,children:t.jsx(s.div,{...l,className:`${n} px-8 py-4 w-full h-full cursor-pointer`,children:a})}):t.jsx(s.button,{...l,type:u,onClick:c,disabled:e,className:`${n} px-8 py-4 ${r}`,children:a})};export{m as P};
