import '../css/panel-toggle-bar.css';
import '../css/gear-panel.css';
import type { InventoryUI } from './InventoryUI';
import type { EquipmentUI } from './EquipmentUI';

export class PanelToggleBar {
    private bar: HTMLDivElement;
    private buttons: Map<string, HTMLButtonElement> = new Map();
    private gearPanel: HTMLDivElement;
    private inventoryUI: InventoryUI;
    private equipmentUI: EquipmentUI;
    private toggleAttributesModal: () => void;

    constructor(
        inventoryUI: InventoryUI,
        equipmentUI: EquipmentUI,
        toggleAttributesModal: () => void,
    ) {
        this.inventoryUI = inventoryUI;
        this.equipmentUI = equipmentUI;
        this.toggleAttributesModal = toggleAttributesModal;
        this.bar = document.createElement('div');
        this.bar.id = 'panel-toggle-bar';
        this.bar.style.display = 'none';

        // Create gear panel wrapper (starts hidden)
        this.gearPanel = document.createElement('div');
        this.gearPanel.id = 'gear-panel';
        this.gearPanel.style.display = 'none';
        const gearCloseBtn = document.createElement('button');
        gearCloseBtn.className = 'gear-panel-close';
        gearCloseBtn.textContent = '×';
        gearCloseBtn.addEventListener('click', () => this.toggleGear());
        this.gearPanel.appendChild(gearCloseBtn);

        // Move inventory and equipment panels into gear panel
        const invEl = this.inventoryUI.getElement();
        const eqEl = this.equipmentUI.getElement();
        
        this.gearPanel.appendChild(eqEl);
        this.gearPanel.appendChild(invEl);
        document.body.appendChild(this.gearPanel);

        // Buttons
        const btnGear = this.createButton(
            'gear-toggle',
            '🎒',
            'Inventário & Equipamento',
            () => this.toggleGear()
        );
        this.buttons.set('gear', btnGear);

        const btnAttributes = this.createButton(
            'attributes-toggle',
            '📋',
            'Atributos',
            () => {
                this.toggleAttributesModal();
            }
        );
        this.buttons.set('attributes', btnAttributes);

        this.bar.appendChild(btnGear);
        this.bar.appendChild(btnAttributes);

        document.body.appendChild(this.bar);
        this.updateButtonStates();
    }

    private createButton(
        id: string,
        icon: string,
        label: string,
        onClick: () => void
    ): HTMLButtonElement {
        const btn = document.createElement('button');
        btn.id = id;
        btn.className = 'panel-toggle-btn';
        btn.title = label;
        btn.innerHTML = `<span class="toggle-icon">${icon}</span>`;
        btn.addEventListener('click', onClick);
        return btn;
    }

    public toggleGear(): void {
        const visible = this.gearPanel.style.display !== 'none';
        if (!visible) {
            this.inventoryUI.getElement().style.display = 'grid';
            this.equipmentUI.getElement().style.display = 'grid';
        }
        this.gearPanel.style.display = visible ? 'none' : 'flex';
        this.updateButtonStates();
    }

    public toggleInventory(): void {
        const el = this.inventoryUI.getElement();
        el.style.display = el.style.display === 'none' ? 'grid' : 'none';
        this.syncGearPanelVisibility();
        this.updateButtonStates();
    }

    public toggleEquipment(): void {
        const el = this.equipmentUI.getElement();
        el.style.display = el.style.display === 'none' ? 'grid' : 'none';
        this.syncGearPanelVisibility();
        this.updateButtonStates();
    }

    private syncGearPanelVisibility(): void {
        const invVisible = this.inventoryUI.getElement().style.display !== 'none';
        const eqVisible = this.equipmentUI.getElement().style.display !== 'none';
        this.gearPanel.style.display = (invVisible || eqVisible) ? 'flex' : 'none';
    }

    private updateButtonStates(): void {
        const gearBtn = this.buttons.get('gear');

        if (gearBtn) {
            gearBtn.classList.toggle('active', this.gearPanel.style.display !== 'none');
        }
    }

    public destroy(): void {
        this.bar.remove();
        this.gearPanel.remove();
    }

    public show(): void {
        this.bar.style.display = 'flex';
    }
}
