import Document, {
    Html,
    Head,
    Main,
    NextScript,
    DocumentContext,
} from "next/document";
import {
    createDOMRenderer,
    renderToStyleElements,
} from "@fluentui/react-components";

class MyDocument extends Document {
    static async getInitialProps(ctx: DocumentContext) {
        const renderer = createDOMRenderer();
        const styles = renderToStyleElements(renderer);
        const initialProps = await Document.getInitialProps(ctx);

        return {
            ...initialProps,
            styles: (
                <>
                    {initialProps.styles}
                    {styles}
                </>
            ),
        };
    }

    render() {
        return (
            <Html lang="en">
                <Head />
                <body>
                    <Main />
                    <NextScript />
                </body>
            </Html>
        );
    }
}

export default MyDocument;
