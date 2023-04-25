"use client";

import {
    FluentProvider,
    GriffelRenderer,
    RendererProvider,
    SSRProvider,
    webLightTheme,
} from "@fluentui/react-components";
import { ReactNode } from "react";

interface FluentRendererProps {
    children: ReactNode;
    renderer: GriffelRenderer;
}

const FluentRenderer: React.FC<FluentRendererProps> = ({
    children,
    renderer,
}) => {
    return (
        <RendererProvider renderer={renderer}>
            <SSRProvider>
                <FluentProvider theme={webLightTheme}>
                    {children}
                </FluentProvider>
            </SSRProvider>
        </RendererProvider>
    );
};

export default FluentRenderer;
