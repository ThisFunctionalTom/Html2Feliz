import { Union, Record } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/Types.js";
import { union_type, record_type, string_type } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/Reflection.js";
import { formatDocument, parse, HtmlDocument$reflection } from "./Html2Feliz.fs.js";
import { reactApi, reactElement, mkAttr } from "./.fable/Feliz.1.16.0/Interop.fs.js";
import { createObj, equals } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/Util.js";
import { singleton, ofArray } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/List.js";
import { join, printf, toText } from "./.fable/fable-library.3.0.0-nagareyama-rc-005/String.js";
import { ProgramModule_run, ProgramModule_withConsoleTrace, ProgramModule_mkSimple } from "./.fable/Fable.Elmish.3.0.0/program.fs.js";
import { Program_withReactSynchronous } from "./.fable/Fable.Elmish.React.3.0.1/react.fs.js";

export class Model extends Record {
    constructor(Input, Output) {
        super();
        this.Input = Input;
        this.Output = Output;
    }
}

export function Model$reflection() {
    return record_type("App.Model", [], Model, () => [["Input", string_type], ["Output", HtmlDocument$reflection()]]);
}

export class Msg extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["InputChanged", "Convert"];
    }
}

export function Msg$reflection() {
    return union_type("App.Msg", [], Msg, () => [[["Item", string_type]], []]);
}

export const example = "\r\n\u003cdiv class=\"container\"\u003e\r\n  \u003cdiv class=\"notification is-primary\"\u003e\r\n    This container is \u003cstrong\u003ecentered\u003c/strong\u003e on desktop and larger viewports.\r\n  \u003c/div\u003e\r\n\u003c/div\u003e\r\n\r\n\u003cnav class=\"level\"\u003e\r\n  \u003cdiv class=\"level-left\"\u003e\r\n    \u003cdiv class=\"level-item\"\u003e\r\n      \u003cp class=\"subtitle is-5\"\u003e\u003cstrong\u003e123\u003c/strong\u003e posts\u003c/p\u003e\r\n    \u003c/div\u003e\r\n    \u003cdiv class=\"level-item\"\u003e\r\n      \u003cdiv class=\"field has-addons\"\u003e\r\n        \u003cp class=\"control\"\u003e\r\n          \u003cinput class=\"input\" type=\"text\" placeholder=\"Find a post\" /\u003e\r\n        \u003c/p\u003e\r\n        \u003cp class=\"control\"\u003e\r\n          \u003cbutton class=\"button\"\u003eSearch\u003c/button\u003e\r\n        \u003c/p\u003e\r\n      \u003c/div\u003e\r\n    \u003c/div\u003e\r\n  \u003c/div\u003e\r\n  \u003cdiv class=\"level-right\"\u003e\r\n    \u003cp class=\"level-item\"\u003e\u003cstrong\u003eAll\u003c/strong\u003e\u003c/p\u003e\r\n    \u003cp class=\"level-item\"\u003e\u003ca\u003ePublished\u003c/a\u003e\u003c/p\u003e\r\n    \u003cp class=\"level-item\"\u003e\u003ca\u003eDrafts\u003c/a\u003e\u003c/p\u003e\r\n    \u003cp class=\"level-item\"\u003e\u003ca\u003eDeleted\u003c/a\u003e\u003c/p\u003e\r\n    \u003cp class=\"level-item\"\u003e\u003ca class=\"button is-success\"\u003eNew\u003c/a\u003e\u003c/p\u003e\r\n  \u003c/div\u003e\r\n\u003c/nav\u003e\r\n";

export function init() {
    return new Model(example, parse(example));
}

export function update(msg, model) {
    if (msg.tag === 1) {
        const Output = parse(model.Input);
        return new Model(model.Input, Output);
    }
    else {
        const content = msg.fields[0];
        return new Model(content, model.Output);
    }
}

export function view(model, dispatch) {
    let arg00, xs, value_2, xs_1, arg00_1, xs_2, value_9, clo1, arg00_2, xs_3, value_13, strings;
    const children = ofArray([(arg00 = (xs = ofArray([mkAttr("rows", 25), mkAttr("cols", 80), (value_2 = model.Input, mkAttr("ref", (e) => {
        let value_4;
        if ((value_4 = (e == null), (!value_4)) ? (!equals(e.value, value_2)) : false) {
            e.value = value_2;
        }
    })), mkAttr("onChange", (ev) => {
        const arg = ev.target.value;
        dispatch((new Msg(0, arg)));
    })]), reactElement("textarea", createObj(xs))), (reactElement("div", createObj(singleton(["children", [arg00]]))))), (xs_1 = ofArray([mkAttr("children", "Convert"), mkAttr("onClick", (_arg1) => {
        dispatch(new Msg(1));
    })]), reactElement("button", createObj(xs_1))), (arg00_1 = (xs_2 = ofArray([mkAttr("rows", 25), mkAttr("cols", 80), (value_9 = (clo1 = toText(printf("%A")), clo1(model.Output)), mkAttr("children", value_9))]), reactElement("textarea", createObj(xs_2))), (reactElement("div", createObj(singleton(["children", [arg00_1]]))))), (arg00_2 = (xs_3 = ofArray([mkAttr("rows", 25), mkAttr("cols", 80), (value_13 = (strings = formatDocument(4, model.Output), (join("\n", strings))), mkAttr("children", value_13))]), reactElement("textarea", createObj(xs_3))), (reactElement("div", createObj(singleton(["children", [arg00_2]])))))]);
    return reactElement("div", createObj(singleton(["children", reactApi.Children.toArray(Array.from(children))])));
}

(function () {
    let program_2;
    let program_1;
    const program = ProgramModule_mkSimple(init, update, view);
    program_1 = Program_withReactSynchronous("feliz-app", program);
    program_2 = ProgramModule_withConsoleTrace(program_1);
    ProgramModule_run(program_2);
})();

