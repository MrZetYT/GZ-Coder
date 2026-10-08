import React, { useState, useEffect, useMemo } from 'react';
import CytoscapeComponent from 'react-cytoscapejs';
import { graphApi } from '../api/graphApi';
import './GraphViewer.css';

const GraphViewer = () => {
    const [originalNodes, setOriginalNodes] = useState([]);
    const [originalEdges, setOriginalEdges] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [filterType, setFilterType] = useState('all');

    const loadGraph = async () => {
        setLoading(true);
        setError(null);
        try {
            const { nodes, edges } = await graphApi.getProjectGraph();
            setOriginalNodes(nodes);
            setOriginalEdges(edges);
        } catch (err) {
            console.error('Failed to load graph:', err);
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    // Фильтрация узлов и рёбер
    const filteredElements = useMemo(() => {
        let filteredNodes = [...originalNodes];

        // Фильтр по типу
        if (filterType !== 'all') {
            filteredNodes = filteredNodes.filter(node => node.type === filterType);
        }

        // Фильтр по поиску
        if (searchTerm.trim() !== '') {
            const term = searchTerm.toLowerCase();
            filteredNodes = filteredNodes.filter(node =>
                node.label && node.label.toLowerCase().includes(term)
            );
        }

        const filteredNodeIds = new Set(filteredNodes.map(node => node.id));

        const filteredEdges = originalEdges.filter(edge =>
            filteredNodeIds.has(edge.source) && filteredNodeIds.has(edge.target)
        );

        const elements = [];

        filteredNodes.forEach(node => {
            elements.push({
                data: {
                    id: node.id,
                    label: node.label,
                    type: node.type,
                },
            });
        });

        filteredEdges.forEach(edge => {
            elements.push({
                data: {
                    source: edge.source,
                    target: edge.target,
                },
            });
        });

        return elements;
    }, [originalNodes, originalEdges, filterType, searchTerm]);

    useEffect(() => {
        loadGraph();
    }, []);

    const layout = {
        name: 'breadthfirst',
        fit: true,
        padding: 30,
        circle: false,
        spacingFactor: 1.5,
        animate: true,
    };

    const stylesheet = [
        {
            selector: 'node[type="file"]',
            style: {
                'background-color': '#4A90E2',
                'label': 'data(label)',
                'color': '#ffffff',
                'font-size': '12px',
                'font-weight': 'bold',
                'text-outline-color': '#1a1a2e',
                'text-outline-width': '2px',
                'shape': 'roundrectangle',
                'width': 'label',
                'padding': '10px',
                'border-width': '1px',
                'border-color': '#ffffff',
            },
        },
        {
            selector: 'node[type="class"]',
            style: {
                'background-color': '#7ED321',
                'label': 'data(label)',
                'color': '#1a1a2e',
                'font-size': '12px',
                'font-weight': 'bold',
                'text-outline-color': '#7ED321',
                'text-outline-width': '1px',
                'shape': 'roundrectangle',
                'width': 'label',
                'padding': '10px',
                'border-width': '1px',
                'border-color': '#ffffff',
            },
        },
        {
            selector: 'node[type="method"]',
            style: {
                'background-color': '#F5A623',
                'label': 'data(label)',
                'color': '#1a1a2e',
                'font-size': '11px',
                'font-weight': 'bold',
                'text-outline-color': '#F5A623',
                'text-outline-width': '1px',
                'shape': 'ellipse',
                'padding': '8px',
                'border-width': '1px',
                'border-color': '#ffffff',
            },
        },
        {
            selector: 'edge',
            style: {
                'width': 2,
                'line-color': '#888888',
                'target-arrow-color': '#888888',
                'target-arrow-shape': 'triangle',
                'curve-style': 'bezier',
                'arrow-scale': 1.2,
            },
        },
        {
            selector: ':selected',
            style: {
                'border-width': '3px',
                'border-color': '#ffffff',
                'line-color': '#ffffff',
                'target-arrow-color': '#ffffff',
            },
        },
    ];

    if (loading) {
        return (
            <div className="graph-loading">
                <div className="spinner"></div>
                <p>Загрузка графа проекта...</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="graph-error">
                <p>❌ Ошибка загрузки графа: {error}</p>
                <button onClick={loadGraph}>Повторить</button>
            </div>
        );
    }

    return (
        <div className="graph-viewer">
            <div className="graph-header">
                <h3>📊 Граф структуры проекта</h3>
                <div className="graph-controls">
                    <div className="search-box">
                        <input
                            type="text"
                            placeholder="🔍 Поиск по названию..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>
                    <div className="filter-buttons">
                        <button
                            className={`filter-btn ${filterType === 'all' ? 'active' : ''}`}
                            onClick={() => setFilterType('all')}
                        >
                            Все
                        </button>
                        <button
                            className={`filter-btn file ${filterType === 'file' ? 'active' : ''}`}
                            onClick={() => setFilterType('file')}
                        >
                            📄 Файлы
                        </button>
                        <button
                            className={`filter-btn class ${filterType === 'class' ? 'active' : ''}`}
                            onClick={() => setFilterType('class')}
                        >
                            🏛️ Классы
                        </button>
                        <button
                            className={`filter-btn method ${filterType === 'method' ? 'active' : ''}`}
                            onClick={() => setFilterType('method')}
                        >
                            🔧 Методы
                        </button>
                    </div>
                    <button onClick={loadGraph} className="refresh-graph-btn" title="Обновить">
                        🔄
                    </button>
                </div>
            </div>
            <div className="graph-container">
                {filteredElements.length > 0 ? (
                    <CytoscapeComponent
                        key={`${searchTerm}-${filterType}`}
                        elements={filteredElements}
                        layout={layout}
                        stylesheet={stylesheet}
                        style={{ width: '100%', height: '100%', minHeight: '600px' }}
                        cy={(cy) => {
                            cy.on('tap', 'node', (evt) => {
                                const node = evt.target;
                                console.log('Clicked node:', node.data());
                            });
                        }}
                    />
                ) : (
                    <div className="graph-empty">
                        <p>Нет данных для отображения графа</p>
                        <p className="graph-hint">
                            {searchTerm ? 'Попробуйте другой поисковый запрос' : 'Загрузите файлы проекта через вкладку "Загрузка проекта"'}
                        </p>
                    </div>
                )}
            </div>
        </div>
    );
};

export default GraphViewer;