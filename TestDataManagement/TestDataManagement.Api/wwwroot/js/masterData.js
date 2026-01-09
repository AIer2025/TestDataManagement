// ==========================================
// 主数据维护 JavaScript
// ==========================================

// 当前选中的标签页
let currentMasterDataTab = 'operator';

// 缓存数据
let systemsCache = [];
let platformsCache = [];

// ==========================================
// 页面切换功能
// ==========================================

// 显示主数据维护页面
function showMasterDataPage() {
    console.log('📋 切换到主数据维护页面...');

    // 更新页面标题
    document.title = '主数据维护 - Weibull 失效数据录入系统';
    const headerTitle = document.querySelector('header h1');
    if (headerTitle) {
        headerTitle.textContent = '⚙️ 主数据维护';
    }

    // 隐藏数据录入页面的main
    const dataEntryMain = document.querySelector('main:not(#weibullAnalysisPage):not(#masterDataPage)');
    if (dataEntryMain) {
        dataEntryMain.style.display = 'none';
    }

    // 隐藏Weibull分析页面
    const weibullPage = document.getElementById('weibullAnalysisPage');
    if (weibullPage) {
        weibullPage.style.display = 'none';
    }

    // 显示主数据维护页面
    const masterDataPage = document.getElementById('masterDataPage');
    if (masterDataPage) {
        masterDataPage.style.display = 'block';
    }

    // 加载当前标签页的数据
    loadMasterDataForTab(currentMasterDataTab);
}

// 返回数据录入页面
function returnToDataEntryFromMasterData() {
    console.log('🔄 返回数据录入页面...');

    // 恢复页面标题
    document.title = 'Weibull 失效数据录入系统';
    const headerTitle = document.querySelector('header h1');
    if (headerTitle) {
        headerTitle.textContent = '📊 Weibull 失效数据录入系统';
    }

    // 显示数据录入页面的main
    const dataEntryMain = document.querySelector('main:not(#weibullAnalysisPage):not(#masterDataPage)');
    if (dataEntryMain) {
        dataEntryMain.style.display = 'block';
    }

    // 隐藏主数据维护页面
    const masterDataPage = document.getElementById('masterDataPage');
    if (masterDataPage) {
        masterDataPage.style.display = 'none';
    }

    // 刷新数据录入页面的下拉列表
    loadModules();
    loadOperators();
}

// 切换主数据标签页
function switchMasterDataTab(tabName) {
    console.log(`📑 切换到标签页: ${tabName}`);
    currentMasterDataTab = tabName;

    // 更新标签页按钮状态
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    event.target.classList.add('active');

    // 更新内容区显示
    document.querySelectorAll('.master-data-content').forEach(content => {
        content.classList.remove('active');
    });
    document.getElementById(`${tabName}Tab`).classList.add('active');

    // 加载对应数据
    loadMasterDataForTab(tabName);
}

// 根据标签页加载数据
function loadMasterDataForTab(tabName) {
    switch (tabName) {
        case 'operator':
            loadOperatorData();
            break;
        case 'system':
            loadSystemData();
            break;
        case 'platform':
            loadPlatformData();
            loadSystemsForDropdown();
            break;
        case 'module':
            loadModuleMasterData();
            loadPlatformsForDropdown();
            break;
    }
}

// ==========================================
// 实验员主数据
// ==========================================

async function loadOperatorData() {
    try {
        console.log('🔄 加载实验员数据...');
        const response = await fetch(`${API_BASE_URL}/api/masterdata/operators`);
        const result = await response.json();

        if (result.success && result.data) {
            renderOperatorTable(result.data);
        } else {
            showMessage('加载实验员数据失败', 'error');
        }
    } catch (error) {
        console.error('❌ 加载实验员数据失败:', error);
        showMessage(`加载失败: ${error.message}`, 'error');
    }
}

function renderOperatorTable(data) {
    const tbody = document.getElementById('operatorTableBody');
    if (!tbody) return;

    if (data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="no-data">暂无数据</td></tr>';
        return;
    }

    tbody.innerHTML = data.map(item => `
        <tr>
            <td>${item.idOperator}</td>
            <td>${item.operatorName || '-'}</td>
            <td>${item.operatorMobile || '-'}</td>
            <td>${item.operatorMail || '-'}</td>
            <td>${item.operatorDepartmentId || '-'}</td>
            <td class="action-buttons">
                <button class="btn btn-edit btn-sm" onclick="editOperator(${item.idOperator})">✏️ 编辑</button>
                <button class="btn btn-danger btn-sm" onclick="deleteOperator(${item.idOperator})">🗑️ 删除</button>
            </td>
        </tr>
    `).join('');
}

function resetOperatorForm() {
    document.getElementById('operatorForm').reset();
    document.getElementById('operatorId').value = '';
    document.getElementById('operatorFormTitle').textContent = '新增实验员';
}

async function editOperator(id) {
    try {
        const response = await fetch(`${API_BASE_URL}/api/masterdata/operators/${id}`);
        const result = await response.json();

        if (result.success && result.data) {
            const item = result.data;
            document.getElementById('operatorId').value = item.idOperator;
            document.getElementById('operatorName').value = item.operatorName || '';
            document.getElementById('operatorMobile').value = item.operatorMobile || '';
            document.getElementById('operatorMail').value = item.operatorMail || '';
            document.getElementById('operatorDeptId').value = item.operatorDepartmentId || '';
            document.getElementById('operatorFormTitle').textContent = '编辑实验员';
            
            // 滚动到表单
            document.getElementById('operatorForm').scrollIntoView({ behavior: 'smooth' });
        }
    } catch (error) {
        showMessage(`获取数据失败: ${error.message}`, 'error');
    }
}

async function deleteOperator(id) {
    if (!confirm('确定要删除该实验员吗？')) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/masterdata/operators/${id}`, {
            method: 'DELETE'
        });
        const result = await response.json();

        if (result.success) {
            showMessage('删除成功', 'success');
            loadOperatorData();
        } else {
            showMessage(result.message || '删除失败', 'error');
        }
    } catch (error) {
        showMessage(`删除失败: ${error.message}`, 'error');
    }
}

// 实验员表单提交
document.addEventListener('DOMContentLoaded', function() {
    const operatorForm = document.getElementById('operatorForm');
    if (operatorForm) {
        operatorForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const id = document.getElementById('operatorId').value;
            const data = {
                operatorName: document.getElementById('operatorName').value.trim(),
                operatorMobile: document.getElementById('operatorMobile').value.trim(),
                operatorMail: document.getElementById('operatorMail').value.trim(),
                operatorDepartmentId: parseInt(document.getElementById('operatorDeptId').value)
            };

            // 验证
            if (!data.operatorName) {
                showMessage('姓名不能为空', 'warning');
                return;
            }
            if (!/^[0-9]{11}$/.test(data.operatorMobile)) {
                showMessage('手机号码必须是11位数字', 'warning');
                return;
            }
            if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(data.operatorMail)) {
                showMessage('请输入正确的邮箱地址', 'warning');
                return;
            }
            if (isNaN(data.operatorDepartmentId) || data.operatorDepartmentId < 1 || data.operatorDepartmentId > 100) {
                showMessage('部门ID必须是1-100之间的数字', 'warning');
                return;
            }

            try {
                const url = id ? `${API_BASE_URL}/api/masterdata/operators/${id}` : `${API_BASE_URL}/api/masterdata/operators`;
                const method = id ? 'PUT' : 'POST';

                const response = await fetch(url, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });

                const result = await response.json();
                if (result.success) {
                    showMessage(id ? '更新成功' : '创建成功', 'success');
                    resetOperatorForm();
                    loadOperatorData();
                } else {
                    showMessage(result.message || '保存失败', 'error');
                }
            } catch (error) {
                showMessage(`保存失败: ${error.message}`, 'error');
            }
        });
    }
});

// ==========================================
// 公司(系统)主数据
// ==========================================

async function loadSystemData() {
    try {
        console.log('🔄 加载公司(系统)数据...');
        const response = await fetch(`${API_BASE_URL}/api/masterdata/systems`);
        const result = await response.json();

        if (result.success && result.data) {
            systemsCache = result.data;
            renderSystemTable(result.data);
        } else {
            showMessage('加载公司(系统)数据失败', 'error');
        }
    } catch (error) {
        console.error('❌ 加载公司(系统)数据失败:', error);
        showMessage(`加载失败: ${error.message}`, 'error');
    }
}

function renderSystemTable(data) {
    const tbody = document.getElementById('systemTableBody');
    if (!tbody) return;

    if (data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="no-data">暂无数据</td></tr>';
        return;
    }

    tbody.innerHTML = data.map(item => `
        <tr>
            <td>${item.systemId}</td>
            <td>${item.systemCode || '-'}</td>
            <td>${item.systemName || '-'}${item.isReferenced ? '<span class="ref-badge">被引用</span>' : ''}</td>
            <td title="${item.description || ''}">${(item.description || '-').substring(0, 20)}${(item.description || '').length > 20 ? '...' : ''}</td>
            <td title="${item.location || ''}">${(item.location || '-').substring(0, 15)}${(item.location || '').length > 15 ? '...' : ''}</td>
            <td><span class="${item.isActive ? 'status-active' : 'status-inactive'}">${item.isActive ? '启用' : '禁用'}</span></td>
            <td class="action-buttons">
                <button class="btn btn-edit btn-sm" onclick="editSystem(${item.systemId})">✏️ 编辑</button>
                <button class="btn btn-danger btn-sm ${item.isReferenced ? 'btn-disabled' : ''}" 
                        onclick="deleteSystem(${item.systemId})" 
                        ${item.isReferenced ? 'disabled title="被平台引用，无法删除"' : ''}>🗑️ 删除</button>
            </td>
        </tr>
    `).join('');
}

function resetSystemForm() {
    document.getElementById('systemForm').reset();
    document.getElementById('systemId').value = '';
    document.getElementById('systemFormTitle').textContent = '新增公司(系统)';
    document.getElementById('systemIsActive').value = 'true';
}

async function editSystem(id) {
    try {
        const response = await fetch(`${API_BASE_URL}/api/masterdata/systems/${id}`);
        const result = await response.json();

        if (result.success && result.data) {
            const item = result.data;
            document.getElementById('systemId').value = item.systemId;
            document.getElementById('systemCode').value = item.systemCode || '';
            document.getElementById('systemName').value = item.systemName || '';
            document.getElementById('systemDescription').value = item.description || '';
            document.getElementById('systemLocation').value = item.location || '';
            document.getElementById('systemInstallDate').value = item.installDate ? item.installDate.split('T')[0] : '';
            document.getElementById('systemWarrantyDate').value = item.warrantyEndDate ? item.warrantyEndDate.split('T')[0] : '';
            document.getElementById('systemIsActive').value = item.isActive ? 'true' : 'false';
            document.getElementById('systemFormTitle').textContent = '编辑公司(系统)';
            
            document.getElementById('systemForm').scrollIntoView({ behavior: 'smooth' });
        }
    } catch (error) {
        showMessage(`获取数据失败: ${error.message}`, 'error');
    }
}

async function deleteSystem(id) {
    if (!confirm('确定要删除该公司(系统)吗？')) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/masterdata/systems/${id}`, {
            method: 'DELETE'
        });
        const result = await response.json();

        if (result.success) {
            showMessage('删除成功', 'success');
            loadSystemData();
        } else {
            showMessage(result.message || '删除失败', 'error');
        }
    } catch (error) {
        showMessage(`删除失败: ${error.message}`, 'error');
    }
}

// 公司(系统)表单提交
document.addEventListener('DOMContentLoaded', function() {
    const systemForm = document.getElementById('systemForm');
    if (systemForm) {
        systemForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const id = document.getElementById('systemId').value;
            const data = {
                systemCode: document.getElementById('systemCode').value.trim(),
                systemName: document.getElementById('systemName').value.trim(),
                description: document.getElementById('systemDescription').value.trim(),
                location: document.getElementById('systemLocation').value.trim(),
                installDate: document.getElementById('systemInstallDate').value || null,
                warrantyEndDate: document.getElementById('systemWarrantyDate').value || null,
                isActive: document.getElementById('systemIsActive').value === 'true'
            };

            // 验证
            if (!data.systemCode) {
                showMessage('系统编码不能为空', 'warning');
                return;
            }
            if (!data.systemName) {
                showMessage('系统名称不能为空', 'warning');
                return;
            }
            if (!data.description) {
                showMessage('系统描述不能为空', 'warning');
                return;
            }
            if (!data.location) {
                showMessage('安装位置不能为空', 'warning');
                return;
            }

            try {
                const url = id ? `${API_BASE_URL}/api/masterdata/systems/${id}` : `${API_BASE_URL}/api/masterdata/systems`;
                const method = id ? 'PUT' : 'POST';

                const response = await fetch(url, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });

                const result = await response.json();
                if (result.success) {
                    showMessage(id ? '更新成功' : '创建成功', 'success');
                    resetSystemForm();
                    loadSystemData();
                } else {
                    showMessage(result.message || '保存失败', 'error');
                }
            } catch (error) {
                showMessage(`保存失败: ${error.message}`, 'error');
            }
        });
    }
});

// ==========================================
// 平台主数据
// ==========================================

async function loadSystemsForDropdown() {
    try {
        if (systemsCache.length === 0) {
            const response = await fetch(`${API_BASE_URL}/api/masterdata/systems`);
            const result = await response.json();
            if (result.success && result.data) {
                systemsCache = result.data;
            }
        }

        const select = document.getElementById('platformSystemId');
        if (select) {
            select.innerHTML = '<option value="">请选择系统</option>';
            systemsCache.forEach(item => {
                const option = document.createElement('option');
                option.value = item.systemId;
                option.textContent = `${item.systemCode} - ${item.systemName}`;
                select.appendChild(option);
            });
        }
    } catch (error) {
        console.error('加载系统下拉列表失败:', error);
    }
}

// 处理平台页面的系统选择变化 - 根据选中的系统过滤平台表格数据
async function handlePlatformSystemChange() {
    const systemId = document.getElementById('platformSystemId').value;
    console.log(`📋 系统选择变化，systemId: ${systemId}`);
    
    if (systemId) {
        // 根据选中的系统ID加载该系统下的平台
        await loadPlatformDataBySystemId(parseInt(systemId));
    } else {
        // 如果没有选择系统，显示所有平台
        await loadPlatformData();
    }
}

// 根据系统ID加载平台数据
async function loadPlatformDataBySystemId(systemId) {
    try {
        console.log(`🔄 加载系统 ${systemId} 下的平台数据...`);
        const response = await fetch(`${API_BASE_URL}/api/masterdata/platforms/by-system/${systemId}`);
        const result = await response.json();

        if (result.success && result.data) {
            // 更新平台缓存（仅为当前系统的平台）
            renderPlatformTable(result.data);
        } else {
            showMessage('加载平台数据失败', 'error');
        }
    } catch (error) {
        console.error('❌ 加载平台数据失败:', error);
        showMessage(`加载失败: ${error.message}`, 'error');
    }
}

async function loadPlatformData() {
    try {
        console.log('🔄 加载平台数据...');
        const response = await fetch(`${API_BASE_URL}/api/masterdata/platforms`);
        const result = await response.json();

        if (result.success && result.data) {
            platformsCache = result.data;
            renderPlatformTable(result.data);
        } else {
            showMessage('加载平台数据失败', 'error');
        }
    } catch (error) {
        console.error('❌ 加载平台数据失败:', error);
        showMessage(`加载失败: ${error.message}`, 'error');
    }
}

function renderPlatformTable(data) {
    const tbody = document.getElementById('platformTableBody');
    if (!tbody) return;

    if (data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="8" class="no-data">暂无数据</td></tr>';
        return;
    }

    tbody.innerHTML = data.map(item => `
        <tr>
            <td>${item.platformId}</td>
            <td>${item.systemName || '-'}</td>
            <td>${item.platformCode || '-'}</td>
            <td>${item.platformName || '-'}${item.isReferenced ? '<span class="ref-badge">被引用</span>' : ''}</td>
            <td>${item.platformType || '-'}</td>
            <td title="${item.description || ''}">${(item.description || '-').substring(0, 15)}${(item.description || '').length > 15 ? '...' : ''}</td>
            <td><span class="${item.isActive ? 'status-active' : 'status-inactive'}">${item.isActive ? '启用' : '禁用'}</span></td>
            <td class="action-buttons">
                <button class="btn btn-edit btn-sm" onclick="editPlatform(${item.platformId})">✏️ 编辑</button>
                <button class="btn btn-danger btn-sm ${item.isReferenced ? 'btn-disabled' : ''}" 
                        onclick="deletePlatform(${item.platformId})"
                        ${item.isReferenced ? 'disabled title="被模块引用，无法删除"' : ''}>🗑️ 删除</button>
            </td>
        </tr>
    `).join('');
}

function resetPlatformForm() {
    document.getElementById('platformForm').reset();
    document.getElementById('platformId').value = '';
    document.getElementById('platformFormTitle').textContent = '新增平台';
    document.getElementById('platformIsActive').value = 'true';
}

async function editPlatform(id) {
    try {
        const response = await fetch(`${API_BASE_URL}/api/masterdata/platforms/${id}`);
        const result = await response.json();

        if (result.success && result.data) {
            const item = result.data;
            document.getElementById('platformId').value = item.platformId;
            document.getElementById('platformSystemId').value = item.systemId;
            document.getElementById('platformCode').value = item.platformCode || '';
            document.getElementById('platformName').value = item.platformName || '';
            document.getElementById('platformType').value = item.platformType || '';
            document.getElementById('platformSerial').value = item.serialNumber || '';
            document.getElementById('platformDescription').value = item.description || '';
            document.getElementById('platformInstallDate').value = item.installDate ? item.installDate.split('T')[0] : '';
            document.getElementById('platformIsActive').value = item.isActive ? 'true' : 'false';
            document.getElementById('platformFormTitle').textContent = '编辑平台';
            
            document.getElementById('platformForm').scrollIntoView({ behavior: 'smooth' });
        }
    } catch (error) {
        showMessage(`获取数据失败: ${error.message}`, 'error');
    }
}

async function deletePlatform(id) {
    if (!confirm('确定要删除该平台吗？')) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/masterdata/platforms/${id}`, {
            method: 'DELETE'
        });
        const result = await response.json();

        if (result.success) {
            showMessage('删除成功', 'success');
            loadPlatformData();
        } else {
            showMessage(result.message || '删除失败', 'error');
        }
    } catch (error) {
        showMessage(`删除失败: ${error.message}`, 'error');
    }
}

// 平台表单提交
document.addEventListener('DOMContentLoaded', function() {
    const platformForm = document.getElementById('platformForm');
    if (platformForm) {
        platformForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const id = document.getElementById('platformId').value;
            const data = {
                systemId: parseInt(document.getElementById('platformSystemId').value),
                platformCode: document.getElementById('platformCode').value.trim(),
                platformName: document.getElementById('platformName').value.trim(),
                platformType: document.getElementById('platformType').value || null,
                serialNumber: document.getElementById('platformSerial').value.trim() || null,
                description: document.getElementById('platformDescription').value.trim(),
                installDate: document.getElementById('platformInstallDate').value || null,
                isActive: document.getElementById('platformIsActive').value === 'true'
            };

            // 验证
            if (!data.systemId || isNaN(data.systemId)) {
                showMessage('请选择所属系统', 'warning');
                return;
            }
            if (!data.platformCode) {
                showMessage('平台编码不能为空', 'warning');
                return;
            }
            if (!data.platformName) {
                showMessage('平台名称不能为空', 'warning');
                return;
            }
            if (!data.description) {
                showMessage('平台描述不能为空', 'warning');
                return;
            }

            try {
                const url = id ? `${API_BASE_URL}/api/masterdata/platforms/${id}` : `${API_BASE_URL}/api/masterdata/platforms`;
                const method = id ? 'PUT' : 'POST';

                const response = await fetch(url, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });

                const result = await response.json();
                if (result.success) {
                    showMessage(id ? '更新成功' : '创建成功', 'success');
                    resetPlatformForm();
                    loadPlatformData();
                } else {
                    showMessage(result.message || '保存失败', 'error');
                }
            } catch (error) {
                showMessage(`保存失败: ${error.message}`, 'error');
            }
        });
    }
});

// ==========================================
// 模块主数据
// ==========================================

async function loadPlatformsForDropdown() {
    try {
        if (platformsCache.length === 0) {
            const response = await fetch(`${API_BASE_URL}/api/masterdata/platforms`);
            const result = await response.json();
            if (result.success && result.data) {
                platformsCache = result.data;
            }
        }

        const select = document.getElementById('modulePlatformId');
        if (select) {
            select.innerHTML = '<option value="">请选择平台</option>';
            platformsCache.forEach(item => {
                const option = document.createElement('option');
                option.value = item.platformId;
                option.textContent = `${item.platformCode} - ${item.platformName}`;
                select.appendChild(option);
            });
        }
    } catch (error) {
        console.error('加载平台下拉列表失败:', error);
    }
}

// 处理平台选择变化 - 更新模块编码前缀并根据平台过滤模块表格数据
async function handleModulePlatformChange() {
    const platformId = document.getElementById('modulePlatformId').value;
    const moduleCodeInput = document.getElementById('moduleMasterCode');
    
    if (platformId) {
        // 从缓存中找到对应的平台，获取platform_code
        const selectedPlatform = platformsCache.find(p => p.platformId == platformId);
        if (selectedPlatform) {
            const platformCode = selectedPlatform.platformCode;
            const prefix = `${platformCode}-`;
            // 如果当前值为空或不以正确前缀开头，则设置前缀
            if (!moduleCodeInput.value || !moduleCodeInput.value.startsWith(prefix)) {
                moduleCodeInput.value = prefix;
            }
            moduleCodeInput.placeholder = `请输入: ${prefix}您的编码`;
            // 保存当前选中的platform_code用于验证
            moduleCodeInput.dataset.platformCode = platformCode;
        }
        
        // 根据选中的平台ID加载该平台下的模块
        await loadModuleDataByPlatformId(parseInt(platformId));
    } else {
        moduleCodeInput.value = '';
        moduleCodeInput.placeholder = '选择平台后自动填充前缀';
        moduleCodeInput.dataset.platformCode = '';
        
        // 如果没有选择平台，显示所有模块
        await loadModuleMasterData();
    }
}

// 根据平台ID加载模块数据
async function loadModuleDataByPlatformId(platformId) {
    try {
        console.log(`🔄 加载平台 ${platformId} 下的模块数据...`);
        const response = await fetch(`${API_BASE_URL}/api/masterdata/modules/by-platform/${platformId}`);
        const result = await response.json();

        if (result.success && result.data) {
            renderModuleMasterTable(result.data);
        } else {
            showMessage('加载模块数据失败', 'error');
        }
    } catch (error) {
        console.error('❌ 加载模块数据失败:', error);
        showMessage(`加载失败: ${error.message}`, 'error');
    }
}

async function loadModuleMasterData() {
    try {
        console.log('🔄 加载模块数据...');
        const response = await fetch(`${API_BASE_URL}/api/masterdata/modules`);
        const result = await response.json();

        if (result.success && result.data) {
            renderModuleMasterTable(result.data);
        } else {
            showMessage('加载模块数据失败', 'error');
        }
    } catch (error) {
        console.error('❌ 加载模块数据失败:', error);
        showMessage(`加载失败: ${error.message}`, 'error');
    }
}

function renderModuleMasterTable(data) {
    const tbody = document.getElementById('moduleMasterTableBody');
    if (!tbody) return;

    if (data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="8" class="no-data">暂无数据</td></tr>';
        return;
    }

    tbody.innerHTML = data.map(item => `
        <tr>
            <td>${item.moduleId}</td>
            <td>${item.platformName || '-'}</td>
            <td>${item.moduleCode || '-'}</td>
            <td>${item.moduleName || '-'}${item.isReferenced ? '<span class="ref-badge">被引用</span>' : ''}</td>
            <td>${item.moduleType || '-'}</td>
            <td>${item.manufacturer || '-'}</td>
            <td><span class="${item.isActive ? 'status-active' : 'status-inactive'}">${item.isActive ? '启用' : '禁用'}</span></td>
            <td class="action-buttons">
                <button class="btn btn-edit btn-sm" onclick="editModuleMaster(${item.moduleId})">✏️ 编辑</button>
                <button class="btn btn-danger btn-sm ${item.isReferenced ? 'btn-disabled' : ''}" 
                        onclick="deleteModuleMaster(${item.moduleId})"
                        ${item.isReferenced ? 'disabled title="被测试数据引用，无法删除"' : ''}>🗑️ 删除</button>
            </td>
        </tr>
    `).join('');
}

function resetModuleMasterForm() {
    document.getElementById('moduleMasterForm').reset();
    document.getElementById('moduleMasterId').value = '';
    document.getElementById('moduleFormTitle').textContent = '新增模块';
    document.getElementById('moduleMasterIsActive').value = 'true';
    document.getElementById('moduleMasterCode').value = '';
    document.getElementById('moduleMasterCode').placeholder = '选择平台后自动填充前缀';
}

async function editModuleMaster(id) {
    try {
        const response = await fetch(`${API_BASE_URL}/api/masterdata/modules/${id}`);
        const result = await response.json();

        if (result.success && result.data) {
            const item = result.data;
            document.getElementById('moduleMasterId').value = item.moduleId;
            document.getElementById('modulePlatformId').value = item.platformId;
            document.getElementById('moduleMasterCode').value = item.moduleCode || '';
            document.getElementById('moduleMasterName').value = item.moduleName || '';
            document.getElementById('moduleMasterType').value = item.moduleType || '';
            document.getElementById('moduleManufacturer').value = item.manufacturer || '';
            document.getElementById('moduleModelNumber').value = item.modelNumber || '';
            document.getElementById('moduleSerialNumber').value = item.serialNumber || '';
            document.getElementById('moduleManufactureDate').value = item.manufactureDate ? item.manufactureDate.split('T')[0] : '';
            document.getElementById('moduleRatedLife').value = item.ratedLife || '';
            document.getElementById('moduleMasterDescription').value = item.description || '';
            document.getElementById('moduleMasterIsActive').value = item.isActive ? 'true' : 'false';
            document.getElementById('moduleFormTitle').textContent = '编辑模块';
            
            document.getElementById('moduleMasterForm').scrollIntoView({ behavior: 'smooth' });
        }
    } catch (error) {
        showMessage(`获取数据失败: ${error.message}`, 'error');
    }
}

async function deleteModuleMaster(id) {
    if (!confirm('确定要删除该模块吗？')) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/masterdata/modules/${id}`, {
            method: 'DELETE'
        });
        const result = await response.json();

        if (result.success) {
            showMessage('删除成功', 'success');
            loadModuleMasterData();
        } else {
            showMessage(result.message || '删除失败', 'error');
        }
    } catch (error) {
        showMessage(`删除失败: ${error.message}`, 'error');
    }
}

// 模块表单提交
document.addEventListener('DOMContentLoaded', function() {
    const moduleMasterForm = document.getElementById('moduleMasterForm');
    if (moduleMasterForm) {
        moduleMasterForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const id = document.getElementById('moduleMasterId').value;
            const platformId = parseInt(document.getElementById('modulePlatformId').value);
            const moduleCode = document.getElementById('moduleMasterCode').value.trim();
            
            const data = {
                platformId: platformId,
                moduleCode: moduleCode,
                moduleName: document.getElementById('moduleMasterName').value.trim(),
                moduleType: document.getElementById('moduleMasterType').value || null,
                manufacturer: document.getElementById('moduleManufacturer').value.trim() || null,
                modelNumber: document.getElementById('moduleModelNumber').value.trim() || null,
                serialNumber: document.getElementById('moduleSerialNumber').value.trim() || null,
                manufactureDate: document.getElementById('moduleManufactureDate').value || null,
                ratedLife: parseInt(document.getElementById('moduleRatedLife').value) || null,
                description: document.getElementById('moduleMasterDescription').value.trim() || null,
                isActive: document.getElementById('moduleMasterIsActive').value === 'true'
            };

            // 验证
            if (!data.platformId || isNaN(data.platformId)) {
                showMessage('请选择所属平台', 'warning');
                return;
            }
            if (!data.moduleCode) {
                showMessage('模块编码不能为空', 'warning');
                return;
            }
            
            // 获取选中平台的platform_code用于验证
            const selectedPlatform = platformsCache.find(p => p.platformId == platformId);
            if (!selectedPlatform) {
                showMessage('无法获取平台信息，请刷新页面重试', 'warning');
                return;
            }
            const platformCode = selectedPlatform.platformCode;
            
            // 验证模块编码格式：必须以 platform_code + "-" 开头
            const expectedPrefix = `${platformCode}-`;
            if (!moduleCode.startsWith(expectedPrefix)) {
                showMessage(`模块编码必须以 "${expectedPrefix}" 开头`, 'warning');
                return;
            }
            if (moduleCode === expectedPrefix) {
                showMessage('请在前缀后输入您的编码', 'warning');
                return;
            }
            
            if (!data.moduleName) {
                showMessage('模块名称不能为空', 'warning');
                return;
            }

            try {
                const url = id ? `${API_BASE_URL}/api/masterdata/modules/${id}` : `${API_BASE_URL}/api/masterdata/modules`;
                const method = id ? 'PUT' : 'POST';

                const response = await fetch(url, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });

                const result = await response.json();
                if (result.success) {
                    showMessage(id ? '更新成功' : '创建成功', 'success');
                    resetModuleMasterForm();
                    loadModuleMasterData();
                } else {
                    showMessage(result.message || '保存失败', 'error');
                }
            } catch (error) {
                showMessage(`保存失败: ${error.message}`, 'error');
            }
        });
    }
});
